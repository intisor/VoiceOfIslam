# UI Elements Not Accounted for in Backend

## 2026-04-17

### 1. Settings Page: Add to Home Screen
- **UI Element:** "Add to Home Screen" button in Settings
- **Backend Support:** None (handled via JS alert, no backend API or manifest/PWA logic currently active)
- **Notes:** PWA/manifest support is ignored per architectural decision; this is a UI-only feature for now.

### 2. Settings Page: Notifications Toggle
- **UI Element:** Notifications toggle in Settings
- **Backend Support:** None (toggle is stored in browser local storage via JS interop, no push notification backend implemented)
- **Notes:** No server-side notification or push subscription logic exists.

### 3. Settings Page: Offline Lectures
- **UI Element:** "Offline Lectures" button in Settings
- **Backend Support:** None (no backend API for offline download or caching of lectures)
- **Notes:** Feature is a UI placeholder; not implemented in backend.

### 4. Settings Page: Listening History
- **UI Element:** "Listening History" button in Settings
- **Backend Support:** None (no backend API or storage for user listening history)
- **Notes:** Feature is a UI placeholder; not implemented in backend.

### 5. Settings Page: About, Support, Sign Out
- **UI Element:** About, Support, and Sign Out buttons
- **Backend Support:** None (About/Support are static, Sign Out is a UI placeholder; no authentication backend)
- **Notes:** No user account or authentication backend exists.

### 6. MainLayout/Settings/SideNav: Daily Verse, Qibla, Donations
- **UI Element:** Side navigation links for "Daily Verse", "Qibla", "Donations"
- **Backend Support:** None (no backend API or logic for these features)
- **Notes:** All are UI placeholders; not implemented in backend.

### 7. Archives Page: Bookmarks
- **UI Element:** Bookmark button for lectures in Archives
- **Backend Support:** None (bookmarks are stored in-memory per session, not persisted or synced)
- **Notes:** No backend API for bookmarks or user profiles.

### 8. Archives Page: Share Button
- **UI Element:** Share button for lectures in Archives
- **Backend Support:** None (uses JS to copy BlobUrl, no backend sharing or link generation)
- **Notes:** UI-only feature.

### 9. NowPlaying Page: Favorite, Playlist, Share
- **UI Element:** Favorite, Playlist, and Share buttons in NowPlaying
- **Backend Support:** None (no backend API for favorites, playlists, or sharing)
- **Notes:** All are UI placeholders; not implemented in backend.

---

Add new UI/backend gaps below this line as they are discovered.
