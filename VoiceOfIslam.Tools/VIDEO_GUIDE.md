# 🎥 Quick Video Guide - Single File Walkthrough

## 🎯 New Approach: Silent Code Walkthrough

**Format**: No narration, just scroll through numbered sections in `Program.cs` with text overlays

**Duration**: 3-4 minutes

**Music**: Lo-fi coding beats (low volume)

---

## 📝 Recording Flow

### Scene 1: Intro Card (5 seconds)
**Screen**: Black background with text
```
Azure Blob Storage Authentication
Two Methods, One File
.NET 10 + C# 13
```

---

### Scene 2: Open Program.cs (5 seconds)
**Action**: Open VS Code, show file explorer with just `Program.cs` (145 lines)

**Overlay**: "Everything in one file"

---

### Scene 3: Scroll Through Numbered Sections (2 minutes)

Pause at each numbered comment block:

#### ── 1. Load config ──
**Overlay**:
```
Reads environment variables
CLI args override defaults
```

#### ── 2. Choose authentication ──
**Overlay**:
```
Ternary decides:
AccountName → Entra ID (secure)
ConnectionString → Account key (fallback)
```

**Highlight**: Lines 19-21 (the ternary)

#### ── 3. Stream blobs ──
**Overlay**:
```
Async stream
No buffering entire blob list
```

#### ── 4. Raw string literal ──
**Overlay**:
```
C# 11+ raw strings
No StringBuilder needed
```

**Highlight**: The `$"""..."""` block

#### ── 5. Write to file ──
**Overlay**:
```
Ready-to-paste SQL output
```

#### ── 6. Parse filename ──
**Overlay**:
```
Extract speaker + title
Handles Episode patterns
```

#### ── 7. Config record ──
**Overlay**:
```
Immutable configuration
Env(key) helper reduces repetition
args is [...] = list patterns
```

---

### Scene 4: Terminal Demo - Method 1 (30 seconds)

**Commands**:
```powershell
$env:AZURE_STORAGE_ACCOUNT_NAME="voiceofislam"
dotnet run --project VoiceOfIslam.Tools
```

**Overlay**:
```
✅ Using Microsoft Entra ID
No secrets needed
```

**Show**: Generated SQL file in VS Code

---

### Scene 5: Terminal Demo - Method 2 (30 seconds)

**Commands**:
```powershell
$env:AZURE_STORAGE_ACCOUNT_NAME=""
$env:AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;..."
dotnet run --project VoiceOfIslam.Tools
```

**Overlay**:
```
⚠️ Using connection string
Contains account key
```

---

### Scene 6: Side-by-Side Comparison (15 seconds)

**Split Screen**:
```
METHOD 1               METHOD 2
────────────────────   ────────────────────
✅ No secrets          ❌ Contains key
✅ Works everywhere    ⚠️ Must rotate
✅ Least privilege     ❌ Full access
```

---

### Scene 7: .NET 10 Features Highlight (20 seconds)

**Screen**: Show before/after code snippets

**Overlay**:
```
Raw strings → No StringBuilder
Collection expressions [] → Cleaner init
List patterns → Simplified args
Local functions → DRY env vars
```

---

### Scene 8: Outro Card (5 seconds)

**Screen**: Black background
```
github.com/intisor/VoiceOfIslam
Single file. Two methods. Zero secrets.
```

---

## 🎨 Visual Style

- **Font**: JetBrains Mono (code), Inter (overlays)
- **Theme**: VS Code Dark+
- **Overlays**: Bottom-third, semi-transparent black background
- **Highlights**: Yellow underline on code sections
- **Transitions**: Smooth fade (0.3s)

---

## 📤 Export Settings

- **Resolution**: 1920x1080 (1080p)
- **Frame Rate**: 60fps
- **Format**: MP4 (H.264)
- **Bitrate**: 8-10 Mbps

---

## 🎵 Music Suggestions

- "Lofi Coding" by Chillhop Music
- "Study Beats" by ChilledCow
- "Dev Mode" by Chillhop Music

**License**: Use royalty-free tracks from YouTube Audio Library or Epidemic Sound

---

## 📱 Social Media

### Title
"Azure Blob Storage Auth in .NET 10 — Single File, Two Methods"

### Description
```
Secure Azure Blob Storage authentication in a single 145-line C# file.

✅ Method 1: Microsoft Entra ID (DefaultAzureCredential)
⚠️ Method 2: Connection String (fallback)

Uses .NET 10 / C# 13 features:
• Raw string literals
• Collection expressions
• List patterns
• Property patterns

No narration — just code walkthrough with overlays.

🔗 Code: github.com/intisor/VoiceOfIslam

#dotnet #csharp #azure #blobstorage #coding
```

### Tags
dotnet, csharp, azure, blob storage, authentication, entra id, visual studio, coding tutorial, clean code, modern csharp

---

## ✅ Checklist

- [ ] Record at 1080p 60fps
- [ ] Add numbered overlay at each section
- [ ] Highlight key code lines (yellow underline)
- [ ] Add lo-fi background music (10-15% volume)
- [ ] Blur any real account names/keys
- [ ] Add chapter markers in YouTube
- [ ] Enable captions
- [ ] Pin comment with GitHub link
