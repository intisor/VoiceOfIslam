# VoiceOfIslam Project Plan

## Purpose
This plan consolidates all proposed feature work based on the gap between intended product scope (PRD + feasibility + phase plan) and what is currently implemented.

## Current State Snapshot
- Foundation in place: .NET 10 Blazor host/client/shared structure, EF Core context, initial `AudioStreams` schema, basic service layer, reconnect modal, blob-to-SQL tooling.
- Major product capabilities still pending: live schedule engine, resilient radio UX, range-streaming proxy, archive pagination UX, in-app nightly blob sync, PWA offline mode, and production hardening.

## Guiding Principles
- Design for weak/mobile networks first.
- Prefer lower egress and lower memory behavior by default.
- Keep the architecture simple (monolith-first, operationally clear).
- Implement security gates before exposure of streaming endpoints.
- Ship incrementally with measurable quality gates.

## Workstreams

### 1. Core Product UX
- Replace placeholder home page with a real entry experience for Live Radio and Archive.
- Add clear status modules for Live/Off Air and next broadcast.
- Add a sticky mini-player for cross-page listening.
- Show now-playing metadata (title, speaker, source).
- Add clear empty states and first-visit onboarding.
- Add explicit data-saver messaging and user guidance.

### 2. Live Radio Experience
- Implement `RadioSchedule` entity and persistence.
- Implement `RadioScheduleService` using UTC computation and WAT display conversion.
- Add 60-second server-side cache for broadcast state.
- Render explicit `LIVE NOW` and `Next Broadcast` states in UI.
- Add stream health indicator (playing/retrying/error/offline).
- Add manual retry action alongside automatic resilience logic.
- Add external-player fallback link for browser compatibility edge cases.

### 3. Audio Streaming Pipeline (HTTP 206)
- Implement secure `/api/audio/stream/{blobName}` endpoint with range support.
- Validate and sanitize blob names (`/`, `..`, `?` rejection).
- Enforce allowlist check against DB (`AudioStreams`).
- Return correct partial content and stream headers.
- Ensure true streaming (no full-file in-memory buffering).
- Add `Cache-Control` policy for replay efficiency.
- Add robust handling for missing/corrupt/unsupported assets.
- Support `HEAD` metadata checks.

### 4. Archive Retrieval and Browsing
- Replace full-table audio query with cursor/seek pagination.
- Add archive indexes for fast seek (`CreatedAt`) and dedupe (`BlobUrl`).
- Implement infinite scroll using `IntersectionObserver`.
- Add search and filters (speaker/date/topic).
- Add sort options and practical metadata chips (size/duration).
- Add lecture details view and related lectures.
- Add recent playback state and resume position support.

### 5. 3G / Dropout Playback Resilience
- Add JS interop resilience module for `stalled`, `playing`, `error` events.
- Separate recoverable network errors from permanent media errors.
- Implement retry strategy with bounded backoff.
- Show reconnecting UI overlay while preserving control accessibility.
- Resume playback flow safely after reconnect.
- Validate behavior with throttled and intermittent network tests.

### 6. PWA and Offline Capability
- Add `manifest.json` with install metadata and icons.
- Add service worker with shell cache versioning.
- Apply cache-first strategy for app shell.
- Apply network-first strategy for list APIs with cache fallback.
- Cache audio stream chunks for previously played content.
- Add offline state indicators and clear UX copy.
- Add cache storage controls and cache clean-up policy.
- Reach and maintain high PWA audit quality.

### 7. Automated Blob Sync Operations
- Implement Quartz `BlobSyncJob` (nightly in-process sync).
- Use async blob enumeration with cooperative yielding.
- Perform check-before-insert dedupe logic.
- Batch inserts and periodic save for efficiency.
- Limit scheduler concurrency to one active job.
- Add structured logging and summary metrics.
- Add manual trigger and dry-run mode for operations.
- Keep `VoiceOfIslam.Tools` path as emergency/manual fallback.

### 8. Data Model Evolution
- Add `BlobName` for secure stream routing.
- Add `FileSizeBytes` for data-size UX and chunk estimates.
- Add `TranscriptVttUrl` reserved for future transcripts.
- Add/confirm required indexes and constraints for scale.
- Add optional taxonomy support (topic/category/language).
- Add auditability fields for ingestion provenance.

### 9. Admin and Editorial Operations
- Create admin pages for schedule management.
- Create admin visibility for sync job status/history.
- Add safe metadata correction and bulk update workflows.
- Add publish/unpublish controls and review states.
- Add role-based access model for operators/editors.
- Add audit trail for admin actions.

### 10. Security and Endpoint Protection
- Harden input validation and access control paths.
- Avoid exposing direct blob URLs in public clients.
- Apply CORS, anti-abuse, and request-throttling controls.
- Enforce secure credential posture (managed identity preferred).
- Add consistent exception handling and security headers baseline.
- Add dependency scanning and remediation workflow.

### 11. Performance and Cost Control
- Track time-to-first-audio under constrained network profiles.
- Track cache hit ratio for stream chunks.
- Monitor query plans and eliminate table scans.
- Add egress and cost dashboards.
- Prefer no-tracking reads and optimized payloads.
- Introduce response caching where safe.
- Define performance budgets and CI regressions checks.

### 12. Observability and Reliability
- Standardize structured logging for app, stream, and jobs.
- Add health checks for DB/blob/scheduler dependencies.
- Add synthetic checks for live stream availability.
- Add alerting for sync failures and elevated stream error rates.
- Add trace correlation across requests and background work.
- Maintain incident runbooks for radio/archive outages.

### 13. Testing Strategy
- Unit tests for schedule logic and timezone correctness.
- Unit tests for filename parsing and metadata derivation.
- Integration tests for range endpoint behavior and headers.
- Integration tests for blob sync batching and dedupe.
- UI tests for Live/Off Air transitions and reconnect overlays.
- Offline/PWA tests for cached shell/list/stream behavior.
- Load tests for concurrent stream requests.
- Acceptance tests covering all integration checklist scenarios.

### 14. Accessibility and Usability
- Ensure keyboard and screen-reader support for player controls and overlays.
- Improve contrast, text readability, and touch targets for mobile.
- Respect reduced-motion preferences in animations.
- Add feedback path for reporting playback issues.
- Add localization-ready formatting and content strategy.

### 15. Community-Centered Product Extensions
- Weekly featured lecture highlights.
- Curated thematic playlists.
- Speaker profile pages and talk collections.
- Share links with time offsets.
- Optional controlled offline downloads.
- Prayer-time aligned recommendation cards.
- Transcript/quote cards in future transcription phases.

## Delivery Model
- Deliver in thin vertical slices with executable quality checks.
- Promote each major capability behind an observable acceptance gate.
- Prefer operational simplicity over premature distribution complexity.

## Definition of Plan Completion
The project plan is complete when all workstreams have:
- A scoped implementation ticket set.
- Explicit acceptance criteria.
- Test coverage expectations.
- Operational readiness and rollback notes.
