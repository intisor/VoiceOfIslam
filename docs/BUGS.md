# Known Bugs and Issues

## 2026-04-17

### 1. Play Now Button Disappearing
- **Symptom:** The "Play Now" button for the live stream would disappear after assets finished loading or on certain UI refreshes.
- **Root Cause:** Blazor UI state was reset or LiveLecture was briefly unavailable during async asset loading, causing the button to not render.
- **Resolution:** The button is now always shown if a live stream is present, and playback falls back to the track's BlobUrl if the API call fails.
- **Status:** Fixed

### 2. Build Error: .pdb File Locked
- **Symptom:** Build failed with error CS2012: Cannot open VoiceOfIslam.Client.pdb for writing.
- **Root Cause:** The .pdb file was locked by a running dotnet process.
- **Resolution:** Killed all dotnet processes and retried the build.
- **Status:** Fixed

### 3. Asset 404s (Icons/PWA)
- **Symptom:** 404 errors for missing icons and PWA manifest files.
- **Root Cause:** Asset references present in code but files missing or not required for current goals.
- **Resolution:** Removed all icon/PWA references from code and ignored them for now.
- **Status:** Fixed

---

Add new bugs/issues below this line as they are discovered.
