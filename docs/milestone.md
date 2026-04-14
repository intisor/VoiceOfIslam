# VoiceOfIslam Milestones (Priority and Order)

This milestone roadmap is intentionally date-free and sequence-based.

## Priority Levels
- P0: Must-have foundation for safe launch.
- P1: Core product completion and quality hardening.
- P2: Operational excellence and scale.
- P3: Growth and advanced capabilities.

## Milestone 1 (P0) — Data Safety and Backend Correctness
**Outcome:** Prevent memory blowups and establish reliable data model/query behavior.

### Scope
- Cursor-based pagination in archive service.
- Add required DB indexes (`CreatedAt`, unique `BlobUrl`).
- Add model fields (`BlobName`, `FileSizeBytes`, `TranscriptVttUrl`).
- Validate migrations are additive and safe.

### Exit Criteria
- Archive retrieval is page-bounded and seek-based.
- Query plans show index-seek behavior.
- Schema is ready for secure stream routing and metadata UX.

## Milestone 2 (P0) — Secure Streaming Pipeline
**Outcome:** Reliable low-data audio playback via HTTP 206 proxy.

### Scope
- Implement `/api/audio/stream/{blobName}` endpoint.
- Add blob name sanitization and DB allowlist gate.
- Add proper cache headers and no full-file buffering.
- Validate partial-content and range request handling.

### Exit Criteria
- Stream endpoint returns expected range behavior.
- Invalid/missing blobs are blocked safely.
- Playback starts quickly under constrained network tests.

## Milestone 3 (P0) — Live Radio State Engine
**Outcome:** Accurate live/off-air rendering based on schedule with timezone correctness.

### Scope
- Implement `RadioSchedule` entity + migration.
- Implement `RadioScheduleService` with UTC logic and WAT output.
- Cache state for 60 seconds.
- Build live page states (`LIVE NOW` vs `Next Broadcast`).

### Exit Criteria
- Live/off-air transitions are correct.
- WAT display remains correct regardless of host timezone.
- UI updates match expected schedule windows.

## Milestone 4 (P1) — Playback Resilience Layer
**Outcome:** Playback auto-recovers from typical mobile network instability.

### Scope
- Implement JS resilience handlers (`stalled`, `playing`, `error`).
- Add reconnect overlay state management.
- Implement bounded retry with safe fallback behavior.

### Exit Criteria
- Mid-playback connectivity loss triggers recover flow.
- Permanent media errors do not loop infinitely.
- User can recover without page refresh in normal failures.

## Milestone 5 (P1) — Automated Blob Sync
**Outcome:** New blobs appear in archive without manual intervention.

### Scope
- Implement Quartz nightly `BlobSyncJob`.
- Add async enumeration, cooperative yielding, and batching.
- Add dedupe logic and structured job logs.
- Add manual trigger and dry-run mode.

### Exit Criteria
- New blob data is ingested automatically.
- Duplicate records are prevented.
- Job execution is observable and operationally safe.

## Milestone 6 (P1) — PWA Offline Experience
**Outcome:** Fast repeat loads and usable offline replay for previously played content.

### Scope
- Add manifest and installability.
- Add service worker with versioned cache lifecycle.
- Cache shell, list API fallbacks, and stream chunks.
- Add offline indicators and update prompts.

### Exit Criteria
- App shell loads offline after first visit.
- Previously played audio can replay with no network.
- Cache invalidation behavior works across updates.

## Milestone 7 (P1) — User Experience Completion
**Outcome:** Product feels complete for core listener journeys.

### Scope
- Real home page and navigation structure.
- Better archive browsing (search, filters, sort).
- Player metadata improvements and resume history.
- Clear empty/error/onboarding states.

### Exit Criteria
- First-run and repeat-run user journeys are coherent.
- Archive discovery is practical on mobile.
- Core actions are achievable with minimal friction.

## Milestone 8 (P2) — Security and Platform Hardening
**Outcome:** Production-ready protections and safer operational posture.

### Scope
- CORS and anti-abuse controls.
- Secure credential pattern standardization.
- Exception handling hardening and security headers.
- Dependency scanning and vulnerability response workflow.

### Exit Criteria
- Public endpoints have explicit guards and limits.
- Secrets are minimized and rotated appropriately.
- Security baseline checks are repeatable.

## Milestone 9 (P2) — Observability and Reliability
**Outcome:** Issues are detected quickly and diagnosable.

### Scope
- Structured logging standards.
- Health checks and synthetic probes.
- Alerts for stream/sync failure modes.
- Correlation IDs and incident runbooks.

### Exit Criteria
- Failures produce actionable telemetry.
- Alert noise is manageable and meaningful.
- Operational playbooks exist for common incidents.

## Milestone 10 (P2) — Quality Engineering and Scale Tests
**Outcome:** Confidence under change and load.

### Scope
- Unit, integration, and acceptance tests for core flows.
- Offline/PWA behavior validation.
- Concurrency/load tests for streaming paths.
- Regression gates in CI.

### Exit Criteria
- Core behavior is covered by automated tests.
- Performance regressions are caught before release.
- Acceptance criteria map directly to test artifacts.

## Milestone 11 (P3) — Admin and Editorial Tooling
**Outcome:** Content operations become self-serve and traceable.

### Scope
- Admin schedule/content management screens.
- Bulk metadata operations.
- Role-based access and action audit trails.

### Exit Criteria
- Routine content operations no longer need engineering intervention.
- Admin actions are auditable and reversible.

## Milestone 12 (P3) — Community Growth Features
**Outcome:** Better engagement beyond baseline playback.

### Scope
- Thematic playlists and featured content.
- Speaker profile pages.
- Share links with timestamp deep-links.
- Optional transcript/quote and recommendation extensions.

### Exit Criteria
- Discovery and engagement metrics improve.
- Advanced features do not degrade core playback reliability.
