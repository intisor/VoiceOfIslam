# Product Requirements Document (PRD): VoiceOfIslam
**Version:** 2.0 — Monolith Rebuild  
**Status:** Active Development  
**Date:** February 2026  
**Repo:** `intisor/VoiceOfIslam`  
**Organisation:** Ahmadiyya Muslim Jamaat Nigeria

---

## 1. Executive Summary

VoiceOfIslam is a **community audio platform** for the Ahmadiyya Muslim Jamaat Nigeria. It serves two, and only two, core purposes:

1. **Live Radio:** Wrap the weekly live Islamic broadcast so members can tune in from any device with a resilient player that holds up on weak Nigerian mobile connections.
2. **Audio Archive:** Browse and listen to past lectures pulled directly from Azure Blob Storage, with bandwidth-efficient streaming and a self-maintaining sync engine.

The previous implementation (`ohunislam`) used YARP, RabbitMQ, and MassTransit — an overengineered distributed system for a use case that needs a single, clean monolith. This rebuild removes all of that.

---

## 2. Background & Problem

**The Distribution Problem:**  
Lectures are currently shared as WhatsApp voice notes or YouTube links — both fragmented, bandwidth-heavy, and unsearchable. Members need one reliable place to tune in live or replay past broadcasts.

**The Bandwidth Reality:**  
Nigerian mobile networks — especially mid-browsing — regularly drop to 50–150 Kbps. Streaming a full MP3 (100MB+) fails halfway and drains data. The platform must protect members' data by design, not as an afterthought.

---

## 3. Goals & Success Metrics

| Metric | Target |
|---|---|
| Audio start time on 3G | < 3 seconds (first chunk) |
| Azure egress cost reduction | 60–70% vs. full-file download |
| App shell load (repeat visit) | < 1 second (PWA cache) |
| Live page correct state (Live/Off Air) | within 60 seconds of actual broadcast state |
| Offline: previously-played tracks | Replay with zero data |

---

## 4. Users

**The Listener** — a Jamaat member on a phone (often Android, MTN/Airtel data) who wants to catch the weekly broadcast or replay a missed lecture. They are not technical. The app must just work, even when the signal is poor.

**The Broadcaster (Sheikh/Admin)** — delivers the weekly broadcast externally (e.g., Icecast/Liquidsoap). No action required in the app — the schedule drives the UI state automatically.

---

## 5. Feature Specifications

### F1 — Weekly Live Radio Player
**Summary:** A Blazor page that knows whether the broadcast is live right now and renders the correct state automatically based on the `RadioSchedules` database table.

**Acceptance Criteria:**
- [ ] When the current UTC time falls within a scheduled broadcast window, the page renders `🔴 LIVE NOW` and embeds the Icecast stream URL in an `<audio>` element.
- [ ] When off air, the page renders `📡 Next Broadcast: [Day] at [Time]` in WAT (UTC+1), regardless of the user's device timezone.
- [ ] Broadcast state is computed server-side and cached for 60 seconds (`IMemoryCache`) — no client-side polling loop.
- [ ] The audio player handles `stalled` and `error` events from the HTML5 audio element: waits 5 seconds then retries `load()` + `play()` automatically.
- [ ] A non-blocking "Reconnecting…" overlay is shown during retry, dismissed on successful reconnection.

**JS Interop Contract (`audioPlayer.js`):**
```javascript
export function initResilienceHandlers(audioElementId, dotNetRef) {
    const audio = document.getElementById(audioElementId);
    let retryTimer = null;

    const retry = () => { audio.load(); audio.play().catch(() => {}); };

    audio.addEventListener('stalled', () => {
        retryTimer = setTimeout(retry, 5000);
        dotNetRef.invokeMethodAsync('OnStreamStall');
    });
    audio.addEventListener('playing', () => {
        if (retryTimer) clearTimeout(retryTimer);
        dotNetRef.invokeMethodAsync('OnStreamPlaying');
    });
    audio.addEventListener('error', () => {
        retryTimer = setTimeout(retry, 10000);
        dotNetRef.invokeMethodAsync('OnStreamError', audio.error?.code ?? -1);
    });
}
```

---

### F2 — Past Audio Archive (Azure Blob)
**Summary:** A paginated browse page listing past lectures pulled from SQL Server, which is itself kept in sync from Azure Blob Storage by a nightly Quartz.NET job.

**Acceptance Criteria:**
- [ ] Archive page shows 20 tracks at a time using cursor-based pagination (not `Skip/Take`).
- [ ] Infinite scroll: when the user reaches the bottom, the next 20 tracks load automatically.
- [ ] Each track streams via the HTTP 206 Range-Request proxy — never a direct Azure Blob URL.
- [ ] Audio starts playing within 3 seconds on a 3G connection.
- [ ] Previously played audio chunks are cached by the PWA Service Worker and replay offline with zero data.

---

### F3 — HTTP 206 Range-Request Proxy Endpoint
**Summary:** A Minimal API endpoint that streams Azure Blob audio in chunks. The browser requests only what it needs — protecting member data and reducing Azure egress costs.

**Acceptance Criteria:**
- [ ] `GET /api/audio/stream/{blobName}` returns HTTP `206 Partial Content`.
- [ ] Endpoint validates `blobName` exists in the `AudioStreams` table before proxying (security gate).
- [ ] Blob name sanitisation: reject values containing `/`, `..`, or query characters.
- [ ] Response includes `Cache-Control: public, max-age=86400, stale-while-revalidate=3600`.
- [ ] Server never buffers the full MP3 in memory — streams directly from Azure SDK to response.

```csharp
app.MapGet("/api/audio/stream/{blobName}", async (
    string blobName, HttpContext ctx,
    BlobServiceClient blobService, AppDbContext db) =>
{
    if (blobName.Contains('/') || blobName.Contains("..")) return Results.BadRequest();
    if (!await db.AudioStreams.AnyAsync(a => a.BlobName == blobName)) return Results.NotFound();

    var blob = blobService.GetBlobContainerClient("archives").GetBlobClient(blobName);
    var range = ctx.Request.Headers.Range;
    var options = new BlobDownloadOptions
    {
        Range = range.Count > 0 ? HttpRange.Parse(range.ToString()) : default
    };
    var download = await blob.DownloadStreamingAsync(options);
    ctx.Response.Headers["Cache-Control"] = "public, max-age=86400, stale-while-revalidate=3600";
    return Results.Stream(download.Value.Content, "audio/mpeg", enableRangeProcessing: true);
});
```

---

### F4 — Quartz.NET Nightly Blob Sync Job
**Summary:** An in-process background job that scans the Azure Blob `archives` container nightly and syncs any new MP3s into SQL Server. Replaces the manual `VoiceOfIslam.Tools` CLI script as the normal operating path.

**Acceptance Criteria:**
- [ ] `BlobSyncJob : IJob` runs via Quartz at 2:00 AM UTC daily (`"0 0 2 * * ?"`).
- [ ] Calls `await Task.Yield()` at start and every 50 blob iterations to avoid thread pool starvation.
- [ ] Check-before-insert: skips blobs already in `AudioStreams` (matched on `BlobUrl`).
- [ ] Inserts in batches of 100 with `SaveChangesAsync()` between batches.
- [ ] Quartz thread pool capped at `MaxConcurrency = 1`.
- [ ] All activity logged via `ILogger<BlobSyncJob>`.

---

### F5 — PWA Offline Mode
**Summary:** The Blazor WASM client registers as a PWA. The app shell and track metadata load instantly on repeat visits. Previously played audio replays offline.

**Acceptance Criteria:**
- [ ] `manifest.json` with correct `name`, icons (192px + 512px), `display: standalone`.
- [ ] Service Worker implements Cache-First for the app shell.
- [ ] Service Worker implements Network-First with Cache Fallback for `/api/audios` (track listing).
- [ ] Audio chunks from `/api/audio/stream/*` are stored in `audio-v1` cache after first play.
- [ ] Lighthouse PWA score ≥ 90.

---

## 6. Database Schema (EF Core)

### `AudioStreams` (existing — additions required)
```csharp
public class AudioStream
{
    [Key] public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(500)]           public string Description { get; set; } = string.Empty;

    [Required] public string BlobUrl { get; set; } = string.Empty;
    public string? BlobName { get; set; }          // ← NEW: for proxy routing & security gate

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledAt { get; set; }

    [MaxLength(100)] public string Speaker { get; set; } = "Unknown";
    public TimeSpan Duration { get; set; }
    public long FileSizeBytes { get; set; }        // ← NEW
    public string? TranscriptVttUrl { get; set; }  // ← NEW: reserved for future Whisper support
}
```

**Required indexes:**
```csharp
e.HasIndex(a => a.CreatedAt).HasDatabaseName("IX_AudioStreams_CreatedAt");    // cursor pagination
e.HasIndex(a => a.BlobUrl).IsUnique().HasDatabaseName("UX_AudioStreams_BlobUrl"); // dedup sync
```

### `RadioSchedules` (new)
```csharp
public class RadioSchedule
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(100)] public string ShowName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string HostName  { get; set; } = string.Empty;

    public DayOfWeek BroadcastDayOfWeek    { get; set; }  // e.g., Monday
    public TimeOnly  BroadcastStartTimeUtc { get; set; }  // stored UTC; display in WAT
    public TimeSpan  ExpectedDuration      { get; set; }  // e.g., 02:00:00

    [Required] public string IcecastStreamUrl { get; set; } = string.Empty;
    public bool     IsActive  { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Live State Logic
```csharp
// RadioScheduleService — always uses UtcNow, never DateTime.Now
public async Task<BroadcastState> GetCurrentBroadcastStateAsync()
{
    return await _cache.GetOrCreateAsync("broadcast_state", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
        var nowUtc   = DateTime.UtcNow;
        var schedule = await _db.RadioSchedules.AsNoTracking()
            .Where(s => s.IsActive && s.BroadcastDayOfWeek == nowUtc.DayOfWeek)
            .FirstOrDefaultAsync();

        if (schedule is null) return BroadcastState.OffAir(...);
        var start = nowUtc.Date + schedule.BroadcastStartTimeUtc.ToTimeSpan();
        var end   = start + schedule.ExpectedDuration;
        return (nowUtc >= start && nowUtc <= end)
            ? BroadcastState.Live(schedule)
            : BroadcastState.OffAir(start + TimeSpan.FromDays(7)); // next week
    });
}
```

---

## 7. Architecture

```
VoiceOfIslam.sln
├── VoiceOfIslam/             # Blazor Server (SSR + Interactive)
│   ├── Components/           # Pages: Radio, Archive, Audio Detail
│   ├── Data/                 # AppDbContext
│   ├── Services/             # AudioService, RadioScheduleService
│   ├── Jobs/                 # BlobSyncJob (Quartz)
│   └── wwwroot/js/           # audioPlayer.js (JS interop)
├── VoiceOfIslam.Client/      # WASM client (interactive components, PWA)
│   └── wwwroot/              # manifest.json, service-worker.js
├── VoiceOfIslam.Shared/      # Models: AudioStream, RadioSchedule
└── VoiceOfIslam.Tools/       # Legacy CLI — kept for emergency manual sync
```

**Technology stack:**

| Layer | Choice |
|---|---|
| Framework | .NET 10 Blazor InteractiveAuto |
| ORM | EF Core 10 + SQL Server |
| Background jobs | Quartz.AspNetCore 3.x |
| Blob storage | Azure.Storage.Blobs + DefaultAzureCredential |
| Caching | IMemoryCache (live state) + HTTP Cache-Control + PWA Service Worker |

---

*VoiceOfIslam — Ahmadiyya Muslim Jamaat Nigeria | February 2026*
