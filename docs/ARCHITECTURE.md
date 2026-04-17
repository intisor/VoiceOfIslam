# Architectural Decisions

## 2026-04-17

### 1. Live Stream Logic (Bond FM)
- **Decision:** The live stream is injected dynamically in AudioService if no DB live stream is present, with a test mode that always shows Bond FM as live.
- **Reasoning:** Ensures a recurring live stream is always available for testing and demo purposes, regardless of DB state.
- **Alternatives Considered:** Static SQL scheduling, but dynamic logic is more flexible for dev/test.

### 2. Asset Management (PWA/Icons)
- **Decision:** All PWA and icon references are ignored in code and not required for current live stream functionality.
- **Reasoning:** User prioritized live stream playback over PWA features and icons.
- **Alternatives Considered:** Full PWA support, but deprioritized for now.

### 3. UI State Handling (Play Now Button)
- **Decision:** The Play Now button is always rendered if a live stream is present, regardless of asset loading state.
- **Reasoning:** Prevents UI flicker/disappearance due to async asset loading or state resets in Blazor.

### 4. Build Stability
- **Decision:** Build errors due to locked .pdb files are resolved by ensuring no dotnet processes are running before building.
- **Reasoning:** Prevents file lock issues and ensures reliable build pipeline.

---

Add new architectural decisions below this line as they are made.
