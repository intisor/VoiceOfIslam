# 🕋 VoiceOfIslam: Full UI Actionability Specification

This document accounts for every single UI element across the application and defines the exact C# backend logic, service interaction, and state management required for their functionality.

---

## 🏗️ 1. Global Core Services (The "Brain")
Every interactive element relies on these four services injected via Dependency Injection (DI).

### `AudioPlayerService`
*   **Property: `CurrentTrack`** → Holds the `AudioStream` currently in memory.
*   **Property: `IsPlaying`** → Boolean state for playback (Syncs all Play/Pause buttons globally).
*   **Property: `Progress`** → Numeric value (0.0 to 1.0) for the current seek position.
*   **Method: `PlayTrack(AudioStream)`** → Switches the track and starts playback.
*   **Method: `TogglePlay()`** → Standard play/pause logic.
*   **Method: `SetProgress(double)`** → Updates playback position (Seeking).

### `LectureService`
*   **Method: `GetAllLectures()`** → Fetches the full library.
*   **Method: `GetLiveLecture()`** → Specifically retrieves the "Monday Live" data with its `ScheduledAt` timestamp.
*   **Method: `GetRecentLectures(count)`** → Fetches latest uploads for the Home screen.
*   **Method: `Search(query)`** → LINQ-based filtering for titles/speakers.

### `SettingsService`
*   **Method: `GetDarkMode()` / `SetDarkMode(bool)`** → Manages logic for the Digital Sanctuary's light/dark mode.
*   **Method: `SetNotifications(bool)`** → Stores preference for live lecture reminders.

---

## 💾 2. Data & Asset Infrastructure
The application's content is powered by a robust backend storage strategy:

### SQL Database (`AppDbContext`)
- **Source of Truth**: The `VoiceOfIslam.Shared.Models.AudioStream` model maps directly to the `AudioStreams` table in SQL Server.
- **Data Ingestion**: A bulk insert script (`SQL/ohunislambulkinsert.sql`) is used to populate the library with verified metadata (Titles, Speakers, and Durations).
- **Service Mapping**: The `LectureService` (Client) is designed to eventually pull directly from this database via a Minimal API layer on the Server.

### Azure Blob Storage
- **Media Hosting**: All `.mp3` audio assets are hosted in the `ohunislam` container on the `intisor` Azure account.
- **URI Format**: `https://intisor.blob.core.windows.net/ohunislam/audio%20files/{filename}.mp3`
- **Streaming**: The Blazor frontend uses these direct Blob URIs for the `<audio>` element, ensuring high-performance streaming directly from Azure's CDN.

---

## 📱 2. Individual Item Mapping (Per Screen)

### A. Global Main Layout (`MainLayout.razor`)
| Item | Interaction Type | Backend logic |
| :--- | :--- | :--- |
| **Menu Toggle** | Icon Click | Calls `MenuService.Toggle()` to slide out the side navigation. |
| **Nav Icons** | Link Click | Uses `NavigationManager` to route between Home, Archive, and Settings. |
| **Mini-Player Title**| Display | Bound to `AudioPlayer.CurrentTrack.Title`. Updates instantly on change. |
| **Mini-Player Play** | Button | Bound to `AudioPlayer.IsPlaying`. Logic: `AudioPlayer.TogglePlay()`. |
| **Mini-Player Bar** | Progress | CSS width bound to `AudioPlayer.Progress * 100`. |

### B. Home Dashboard (`Home.razor`)
| Item | Interaction Type | Backend logic |
| :--- | :--- | :--- |
| **Live Title** | Data Bind | Pulls from `LectureService.GetLiveLecture().Title`. |
| **Countdown Timer** | Reactive UI | A C# `Timer` runs every minute, calculating `ScheduledTime - Now`. |
| **Set Reminder** | Button | JS Interop to trigger a browser notification or alert. |
| **Recommended Card**| Click | Calls `AudioPlayer.PlayTrack(thisLecture)`. Navigates to the player. |
| **Search Icon** | Click | Focuses search input or routes to the Archives/Search page. |

### C. Archives Library (`Archives.razor`)
| Item | Interaction Type | Backend logic |
| :--- | :--- | :--- |
| **Filter Chips** | Click | Updates `currentFilter` variable. UI re-renders with filtered LINQ list. |
| **Lecture Item** | Container Click| Calls `AudioPlayer.PlayTrack(item)` & switches global audio context. |
| **Bookmark Icon** | Button | Adds item ID to a "Saved" collection in the backend database/local storage. |
| **Share Icon** | Button | Invokes the `WebShareAPI` via JS Interop to share text/URL. |
| **Stats (Total)** | Bound Text | Bound to `filteredLectures.Count` to show real-time library depth. |

### D. Now Playing Screen (`NowPlaying.razor`)
| Item | Interaction Type | Backend logic |
| :--- | :--- | :--- |
| **Expand More Icon**| Click | Navigates "Back" to previous page using `NavigationManager`. |
| **Favorite** | Toggle | Boolean `isFavorite` mapped to user profile storage. |
| **Seek Slider** | Input/Slide | Calls `AudioPlayer.SetProgress(e.Value)`. Updates the audio timestamp. |
| **Replay/Forward** | Button | Arithmetic: `AudioPlayer.Progress +/- offset_seconds`. |
| **Center Play** | Main Toggle | Large Button bound to `AudioPlayer.IsPlaying` state. |
| **Playback Speed** | Cycle Button | Cycles float values `[1.0, 1.25, 1.5, 2.0]` into the audio engine. |

### E. Settings Page (`Settings.razor`)
| Item | Interaction Type | Backend logic |
| :--- | :--- | :--- |
| **PWA Install** | Button | Triggers the browser's `beforeinstallprompt` event via JS. |
| **Theme Switch** | Toggle | Calls `SettingsService.ToggleTheme()`. Injects `.dark` into the DOM. |
| **Notification SW** | Switch | Updates `SettingsService.NotificationsEnabled` and persists to storage. |
| **Offline Link** | Link | Navigates to a subview showing cached lectures available without internet. |
| **Sign Out** | Button | Clears session state and redirects to the landing page. |

---

## 🔄 3. Interaction Flow Example
1.  **User Action**: Clicks "Faith" filter in Archives.
2.  **C# Logic**: `currentFilter` updates → `filteredLectures` re-calculates via LINQ.
3.  **UI Result**: The list of cards instantly shrinks to only show "Faith" related lectures.
4.  **Audio Interaction**: User clicks a card → `AudioPlayer.PlayTrack()` is called → **Mini-Player** on all pages starts showing the new title.
