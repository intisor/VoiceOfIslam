# Phase 1: MVP & Core Stability — VoiceOfIslam
**Version:** 2.0  
**Date:** February 2026  
**Organisation:** Ahmadiyya Muslim Jamaat Nigeria  
**Sprint Cadence:** 2-week sprints | **Target:** 6 weeks (3 sprints)

---

## GitHub Issues — VS Code Copilot Chat Prompt

```
@workspace /new
Read docs/Phase1-Project-Plan.md. For each top-level task (- [ ] checkbox), create a GitHub Issue
in intisor/VoiceOfIslam with the bold text as the title and the full description as the body.
Add labels matching the Category value. Then add each issue to the GitHub Project "VoiceOfIslam".
```

---

## Sprint 1 — Data & Backend Foundation (Weeks 1–2)

> **Goal:** Fix the database memory threat and get the nightly blob sync running end-to-end.

---

- [ ] **Task 1: Fix O(N) Memory Leak — Cursor-Based Pagination**

  `AudioService.GetPastAudios()` currently calls `.ToListAsync()` on the full `AudioStreams` table with no pagination. As the lecture archive grows this will crash the browser. Replace it with cursor-based seek pagination returning 20 records at a time, with infinite scroll on the archive Blazor page.

  **Steps:**
  1. Rename method to `GetPastAudiosAsync(DateTime? cursor, int pageSize = 20)`.
  2. Add `.Where(a => a.CreatedAt < cursor)` seek clause; keep `.OrderByDescending(a => a.CreatedAt).Take(pageSize)`.
  3. EF migration: add `IX_AudioStreams_CreatedAt` non-clustered index.
  4. EF migration: add `UX_AudioStreams_BlobUrl` unique index (also needed by the sync job).
  5. Update the archive page: use JS `IntersectionObserver` to trigger next-page load on scroll bottom.
  6. Verify SQL plan shows "Index Seek" not "Table Scan" via `SET STATISTICS IO ON`.

  - **Category:** `🏗️ Core Stability`
  - **Estimated Effort:** `4–6 hours`

---

- [ ] **Task 2: Extend AudioStream Schema & Migrate**

  The `AudioStream` model needs three new fields required by the HTTP 206 proxy, the sync job, and future Whisper support. Add them now to avoid a disruptive migration later.

  **Fields to add:**
  - `string? BlobName` — the blob filename (e.g. `lecture-2024-01-15.mp3`); used by the proxy endpoint for routing and the security allowlist check.
  - `long FileSizeBytes` — used by the UI to show file size and estimate chunk count.
  - `string? TranscriptVttUrl` — reserved for future Whisper transcription; nullable, ignored for now.

  **Steps:**
  1. Add the three properties to `VoiceOfIslam.Shared/Models/AudioStream.cs`.
  2. `dotnet ef migrations add AddAudioStreamExtendedFields`.
  3. Verify the migration SQL is `ALTER TABLE ADD COLUMN` only — no data loss.
  4. `dotnet ef database update`.

  - **Category:** `🏗️ Core Stability`
  - **Estimated Effort:** `1–2 hours`

---

- [ ] **Task 3: Implement BlobSyncJob (Quartz.NET)**

  Replace the manual `VoiceOfIslam.Tools` CLI script with an in-process Quartz `IJob` that runs nightly at 2:00 AM, scans the Azure Blob `archives` container, and upserts new records into SQL Server — with no thread pool starvation.

  **Steps:**
  1. Create `VoiceOfIslam/Jobs/BlobSyncJob.cs` implementing `IJob`.
  2. `await Task.Yield()` at the top of `Execute()` before any I/O.
  3. Enumerate blobs via `containerClient.GetBlobsAsync()` (async only — never the sync overload).
  4. `await Task.Yield()` every 50 iterations inside the loop.
  5. Check-before-insert: `AnyAsync(a => a.BlobUrl == url)` → skip if exists.
  6. Batch inserts: accumulate 100, then `AddRangeAsync` + `SaveChangesAsync`, then clear.
  7. Register in `Program.cs` with cron `"0 0 2 * * ?"` and `MaxConcurrency = 1`.
  8. Add `ILogger<BlobSyncJob>` structured logging throughout.
  9. Test: temporarily set cron to fire 60 seconds from now, confirm records appear in DB.

  - **Category:** `🏗️ Core Stability`
  - **Estimated Effort:** `5–8 hours`

---

## Sprint 2 — Audio Streaming & Live Radio (Weeks 3–4)

> **Goal:** Ship the core audio experience — range-request proxy, live radio state, and the 3G resilience layer.

---

- [ ] **Task 4: HTTP 206 Range-Request Audio Proxy Endpoint**

  Build the Minimal API endpoint that proxies audio playback to Azure Blob Storage in chunks. This is the single highest-impact feature for data saving and dropout resilience.

  **Steps:**
  1. Register `BlobServiceClient` in DI using `DefaultAzureCredential` (see `VoiceOfIslam.Tools/COMPARISON.md`).
  2. `app.MapGet("/api/audio/stream/{blobName}", ...)` in `Program.cs`.
  3. Security gate: `db.AudioStreams.AnyAsync(a => a.BlobName == blobName)` → 404 if not found.
  4. Sanitise: return `Results.BadRequest()` if `blobName` contains `/`, `..`, or `?`.
  5. Forward the `Range` header to `blob.DownloadStreamingAsync(options)`.
  6. Return `Results.Stream(..., enableRangeProcessing: true)`.
  7. Add `Cache-Control: public, max-age=86400, stale-while-revalidate=3600` header.
  8. Confirm response is `206 Partial Content` in Chrome DevTools Network tab.
  9. Test under Chrome 3G throttle: audio starts within 3 seconds.

  - **Category:** `🎵 Audio Streaming`
  - **Estimated Effort:** `4–6 hours`

---

- [ ] **Task 5: RadioSchedule Entity & Live State Engine**

  Create the `RadioSchedules` table and the `RadioScheduleService` that computes whether the broadcast is live right now, cached server-side for 60 seconds.

  **Steps:**
  1. Create `VoiceOfIslam.Shared/Models/RadioSchedule.cs` (schema in PRD §6).
  2. Create `VoiceOfIslam.Shared/Models/BroadcastState.cs` (record: `IsLive`, `IcecastUrl`, `NextBroadcastWat`).
  3. Add `DbSet<RadioSchedule> RadioSchedules` to `AppDbContext`.
  4. `dotnet ef migrations add AddRadioSchedules` + `database update`.
  5. Seed the Monday 8:00 PM WAT schedule (= `19:00` UTC stored in `BroadcastStartTimeUtc`).
  6. Create `RadioScheduleService` using `DateTime.UtcNow` only — never `DateTime.Now`.
  7. Cache result in `IMemoryCache` with 60-second absolute expiry.
  8. Register `IMemoryCache` + service in `Program.cs`.
  9. Build the Blazor Radio page: render `🔴 LIVE NOW` vs `📡 Next Broadcast: [day] [time] WAT`.
  10. Display time always in **WAT (UTC+1)** using `TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")`.
  11. Smoke test: seed a schedule starting 5 min from now; verify page state transitions correctly.

  - **Category:** `📻 Live Radio`
  - **Estimated Effort:** `4–6 hours`

---

- [ ] **Task 6: Audio Player 3G Resilience (JS Interop)**

  Implement the `audioPlayer.js` JavaScript module that handles `stalled` and `error` events and auto-retries without user intervention. Wire it to a Blazor "Reconnecting…" overlay via `[JSInvokable]` callbacks.

  **Steps:**
  1. Create `VoiceOfIslam/wwwroot/js/audioPlayer.js` as an ES Module.
  2. Export `initResilienceHandlers(audioElementId, dotNetRef)` as specified in PRD §5 (F1).
  3. `stalled`: set 5-second timeout → `audio.load()` + `audio.play()` + invoke `OnStreamStall`.
  4. `playing`: clear timeout + invoke `OnStreamPlaying`.
  5. `error code 2` (network): 10-second retry + invoke `OnStreamError`.
  6. `error code 4` (bad URL): invoke `OnStreamPermanentError` — no retry.
  7. Wire to Blazor via `IJSRuntime.InvokeVoidAsync("import", "/js/audioPlayer.js")`.
  8. Implement `[JSInvokable]` methods on the Blazor component for each callback.
  9. Show/hide "Reconnecting…" overlay on `OnStreamStall` / `OnStreamPlaying`.
  10. Test: Chrome DevTools → Offline mid-playback → overlay appears → Online → overlay dismisses.

  - **Category:** `📻 Live Radio`
  - **Estimated Effort:** `5–7 hours`

---

## Sprint 3 — PWA & Polish (Weeks 5–6)

> **Goal:** Ship offline capability and confirm the full integration works end-to-end.

---

- [ ] **Task 7: PWA Offline Mode (Service Worker + Manifest)**

  Register the Blazor WASM client as a PWA so the app shell and track listing load instantly on repeat visits, and previously played audio replays with zero data.

  **Steps:**
  1. Create `VoiceOfIslam.Client/wwwroot/manifest.json` with `name`, `short_name`, icons (192px + 512px), `display: standalone`, `start_url: "/"`.
  2. Create icon assets (SVG → PNG via online tool or `generate_image`).
  3. Create `service-worker.js` with `CACHE_VERSION = 'shell-v1'`.
  4. `install`: cache app shell files (`index.html`, `.css`, `.js`, `manifest.json`); call `self.skipWaiting()`.
  5. `activate`: delete all caches not matching `CACHE_VERSION`; call `self.clients.claim()`.
  6. `fetch` handler — Cache-First for app shell; Network-First for `/api/audios*`; Cache-First for `/api/audio/stream/*`.
  7. Store audio stream responses in `audio-v1` cache partition after first play.
  8. Add `<link rel="manifest">` to `index.html`.
  9. Run Lighthouse PWA audit → target score ≥ 90.
  10. Test offline: load site, go offline in DevTools, refresh → archive list loads from cache.

  - **Category:** `📵 Offline / PWA`
  - **Estimated Effort:** `5–8 hours`

---

## Phase 1 Integration Checklist

Phase 1 is done when all tasks are complete **and** these integration scenarios pass:

- [ ] **IC-1:** A new MP3 dropped into the Azure Blob container before 2:00 AM appears on the archive page by morning, without any manual action.
- [ ] **IC-2:** On a Chrome DevTools 3G throttle, a lecture starts playing within 5 seconds, and does not freeze permanently on a simulated 10-second network dropout.
- [ ] **IC-3:** The Radio page shows `🔴 LIVE NOW` during the schedule window and `📡 Next Broadcast: Monday at 8:00 PM WAT` outside of it.
- [ ] **IC-4:** Going offline in DevTools after a previous visit still shows the archive track listing and replays a previously played lecture with zero network activity.

---

*VoiceOfIslam — Ahmadiyya Muslim Jamaat Nigeria | February 2026*
