# Feasibility & Architecture Study: VoiceOfIslam
**Version:** 2.0  
**Date:** February 2026  
**Organisation:** Ahmadiyya Muslim Jamaat Nigeria

---

## 1. Feasibility Verdict — HIGH ✅

The VoiceOfIslam monolith is technically straightforward. The two core features (live radio wrapper + blob archive player) map cleanly onto a single Blazor app with EF Core, Quartz.NET, and the Azure Blob SDK. No exotic dependencies. All risks below are avoidable implementation-level issues.

| Area | Risk |
|---|---|
| .NET 10 Blazor InteractiveAuto | 🟢 Low — GA, stable |
| Quartz.AspNetCore in-process | 🟢 Low — mature, widely deployed |
| HTTP 206 Minimal API | 🟢 Low — standard HTTP spec |
| DefaultAzureCredential / Managed Identity | 🟢 Low — production-ready |
| PWA Service Worker cache management | 🟡 Medium — requires versioning discipline |
| 3G audio resilience (JS Interop) | 🟡 Medium — implementable, needs field testing |

---

## 2. Why the Old Architecture Was Wrong

The `ohunislam` system used YARP, RabbitMQ, and MassTransit. Each was solving a problem that doesn't exist here:

- **YARP** routes traffic between services. There is one service. There is nothing to route between.
- **RabbitMQ** fans out async messages between producers and consumers. The only "producer" is the nightly sync job; the only "consumer" is the SQL database. An in-process method call does the same job for free.
- **MassTransit** manages complex saga patterns across distributed services. There are no sagas. There are no distributed services.

**Elimination cost savings (rough estimates):**

| Removed component | Azure cost saved |
|---|---|
| YARP gateway VM | ~$15/mo |
| RabbitMQ VM | ~$15/mo |
| Extra backend service VM | ~$15/mo |
| **Total** | **~$45/mo** |

The replacement: one App Service (Basic B1, ~$13/mo) + one SQL Database (Basic, ~$5/mo).

---

## 3. Threat: The WASM Memory Bomb

**File:** `VoiceOfIslam/Services/AudioService.cs`, Line 19  
**Severity:** 🔴 Critical — fix before production

```csharp
// CURRENT — will crash the browser tab as the archive grows
return await context.AudioStreams.AsNoTracking().ToListAsync();
```

This loads the entire `AudioStreams` table into JavaScript heap on every page load. At 1,000 records it sends ~2MB of JSON to the WASM client. At 5,000 records a cheap Android phone's browser tab will die.

**Fix — Cursor-Based Pagination:**
```csharp
public async Task<List<AudioStream>> GetPastAudiosAsync(DateTime? cursor, int pageSize = 20)
{
    await using var context = _dbFactory.CreateDbContext();
    var q = context.AudioStreams.AsNoTracking().OrderByDescending(a => a.CreatedAt);
    if (cursor.HasValue)
        q = (IOrderedQueryable<AudioStream>)q.Where(a => a.CreatedAt < cursor.Value);
    return await q.Take(pageSize).ToListAsync();
}
```
This always returns exactly 20 rows via an index seek, regardless of how many total records exist. Add `IX_AudioStreams_CreatedAt` index via EF migration.

---

## 4. Threat: Quartz Thread Pool Starvation

**Severity:** 🔴 Critical — the Quartz job runs on the same thread pool as web requests

If `BlobSyncJob.Execute()` blocks threads synchronously during blob enumeration, web requests queue and time out. The sync job could be processing 1,000+ blobs at 2:00 AM — if even one member is actively using the app at that time, they'll see timeouts.

**Fix — Cooperative Yielding:**
```csharp
public async Task Execute(IJobExecutionContext context)
{
    await Task.Yield(); // Release thread pool before I/O begins
    int count = 0;
    await foreach (var blob in containerClient.GetBlobsAsync())
    {
        if (++count % 50 == 0) await Task.Yield(); // Yield every 50 items
        // ... upsert logic
    }
}
```
Also: set `MaxConcurrency = 1` on the Quartz thread pool in `Program.cs` to prevent concurrent runs.

---

## 5. Threat: Azure Egress Cost Without Range Requests

A 2-hour lecture at 128 Kbps ≈ 115 MB. Without HTTP 206, the browser tries to download the entire file upfront. On a 3G connection that drops at 20% — the member gets 0 seconds of audio and the server has already sent 23 MB of chargeable egress.

**With HTTP 206 + Cache-Control:**
- Browser fetches ~512 KB at a time.
- `Cache-Control: public, max-age=86400` means a second play is served from browser disk cache — **₦0 egress**.
- A dropped connection at 20% wastes 512 KB maximum instead of 23 MB.

**Egress projection:**

| Scenario | Monthly egress | Azure cost |
|---|---|---|
| No H206, no cache | ~100 GB | ~$8.70 |
| H206 + Cache-Control | ~10 GB | ~$0.87 |

---

## 6. Threat: 3G Audio Player Silently Freezing

Default HTML5 `<audio>` behaviour on connection loss: the element enters `stalled` state, pauses, and shows nothing. Members see a frozen play button and assume the stream is down. They leave.

**The 3-layer fix:**
1. **JS Interop:** Listen to `stalled` + `error` events. Wait 5–10 seconds, then call `audio.load()` + `audio.play()` automatically.
2. **UI overlay:** Show a non-blocking "Reconnecting…" badge while the retry is in progress.
3. **HTTP 206:** Chunks naturally limit how much is wasted per dropout — no large partial downloads.

The JS module must handle two distinct cases:
- `stalled` / `error code 2` (network loss) → retry with backoff, no limit
- `error code 4` (bad URL / unsupported format) → show permanent error, do not retry

---

## 7. Threat: Timezone Display Bug

Storing "Monday 8:00 PM" without timezone context will show incorrect times the moment the server is deployed outside WAT (UTC+1), or when a member views the page with a non-Nigerian device timezone.

**Rule: store UTC, display WAT:**
```csharp
// Server: always compare in UTC
var nowUtc = DateTime.UtcNow; // Never DateTime.Now

// Display: convert at the last moment, in WAT
private static readonly TimeZoneInfo Wat =
    TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");

string DisplayWat(DateTime utc) =>
    TimeZoneInfo.ConvertTimeFromUtc(utc, Wat).ToString("dddd 'at' h:mm tt 'WAT'");
```
Nigeria does not observe DST — `W. Central Africa Standard Time` is a fixed UTC+1 offset, so this is always stable.

---

## 8. Threat: PWA Cache Invalidation (Stale App)

If the Service Worker caches the app shell and a bug fix is deployed, returning members will be served the old broken version indefinitely.

**Fix:**
```javascript
// service-worker.js — increment CACHE_VERSION on every deploy
const CACHE_VERSION = 'shell-v1';

self.addEventListener('install',  e => { e.waitUntil(/* open CACHE_VERSION, add shell files */); self.skipWaiting(); });
self.addEventListener('activate', e => {
    e.waitUntil(caches.keys().then(keys =>
        Promise.all(keys.filter(k => k !== CACHE_VERSION).map(k => caches.delete(k)))
    ));
    self.clients.claim();
});
```
`skipWaiting()` forces immediate activation. `clients.claim()` takes control of all open tabs. Old caches are deleted on activate.

---

## 9. Security: Proxy Endpoint

The `/api/audio/stream/{blobName}` endpoint must not be an open proxy to any Azure Blob. Two mandatory gates:

1. **Database allowlist:** Only serve blobs that exist in `AudioStreams.BlobName`. Return 404 for anything else.
2. **Name sanitisation:** Reject `blobName` values containing `/`, `..`, or `?` (path traversal prevention).

The `DefaultAzureCredential` approach (already documented in `VoiceOfIslam.Tools/COMPARISON.md`) means no connection string or account key is ever in code or config. On Azure App Service, enable System-Assigned Managed Identity and assign `Storage Blob Data Reader` on the container.

---

## 10. Infrastructure Cost Summary

| Resource | Tier | Monthly Cost |
|---|---|---|
| Azure App Service | Basic B1 | ~$13 |
| Azure SQL Database | Basic (5 DTU) | ~$5 |
| Azure Blob Storage (100 GB) | LRS Hot | ~$2 |
| Azure Egress (with H206+cache) | ~10 GB | ~$0.87 |
| **Total** | | **~$21/mo** |

vs. the old `ohunislam` multi-VM setup: **~$60+/mo**. This rebuild saves approximately **$40/mo (₦64,000/mo)** with full feature parity.

---

*VoiceOfIslam — Ahmadiyya Muslim Jamaat Nigeria | February 2026*
