# 🎬 Recording Checklist — Visual Studio Step-Into

## Pre-Recording Setup

- [ ] Open `VoiceOfIslam.Tools` in Visual Studio
- [ ] Update `launchSettings.json` with your real account name / connection string
- [ ] Run `az login` in terminal (for Method 1)
- [ ] Set all breakpoints below (F9 on each line)
- [ ] Set zoom: **View → Zoom In** to ~150%
- [ ] Dock **Locals** and **Watch** windows at the bottom
- [ ] Open Azure Portal tabs:
  - Storage Account → Access Control (IAM)
  - Storage Account → Access keys

---

## Breakpoints to Set in Program.cs

| # | Line | What debugger shows |
|---|------|---------------------|
| 1 | `var options = ScriptOptions.FromEnvironment(args)` | Hover over `options` → all config loaded from env |
| 2 | `if (options.AccountName is null ...` | Hover → which auth method will be used |
| 3 | `var container = options.AccountName is { } account` | Step into ternary → watch which branch executes |
| 4 | `Console.WriteLine(options.AccountName is not null` | Locals shows `container` with its URI |
| 5 | `List<string> rows = []` | Watch window: add `rows` |
| 6 | `var (speaker, title) = ParseFileName(...)` | F11 → step INTO ParseFileName, see each parse step |
| 7 | `rows.Add(...)` | Hover over `rows` → watch SQL rows building up |
| 8 | `var sql = rows.Count == 0 ...` | Hover `sql` → see the full raw string output |
| 9 | `await File.WriteAllTextAsync(outputPath, sql)` | Hover `outputPath` → shows the output file location |

---

## Recording Flow

### 🔴 Take 1: Method 1 (Entra ID)

1. Select **"Method 1 - Entra ID"** in the debug profile dropdown
2. Press **F5** to start
3. At each breakpoint:
   - Hover over key variables
   - Let the **Locals** window sit visible
   - **F10** to step over, **F11** to step into
4. At breakpoint 3 — show the ternary taking the **Entra ID** branch
5. At breakpoint 6 — **F11** into `ParseFileName`, show the Episode/honorific logic
6. After final breakpoint — open the generated `.sql` file

### 🔴 Take 2: Method 2 (Connection String)

1. Select **"Method 2 - Connection String"** in the debug profile dropdown
2. Press **F5** to start
3. At breakpoint 3 — show the ternary taking the **Connection String** branch
4. Everything else identical

---

## What the Viewer Sees Without Any Narration

| Moment | Visual cue | Overlay text to add in edit |
|--------|------------|-----------------------------|
| Breakpoint 1 | `options` expanded in Locals | `Env vars loaded into config` |
| Breakpoint 3 (Method 1) | Ternary → Entra ID branch lit up | `AccountName set → Entra ID` |
| Breakpoint 3 (Method 2) | Ternary → Connection String branch lit up | `No AccountName → Connection string` |
| Breakpoint 6 | Step into ParseFileName | `Extracts speaker + title` |
| Breakpoint 7 | `rows` growing in Watch | `Building SQL rows per blob` |
| Breakpoint 8 | `sql` raw string expanded | `C# 11 raw string literal` |
| Breakpoint 9 | `outputPath` value visible | `File written ✅` |

---

## Visual Studio Tips for Recording

- **Pin variables**: Hover → click 📌 to pin value next to the line
- **Watch window**: Add `rows.Count`, `options.AccountName`, `sql` to Watch
- **Autos window**: Shows variables used in current and previous line automatically
- **Step shortcuts**:
  - `F10` — Step Over (stay in current method)
  - `F11` — Step Into (go inside called method)
  - `F5`  — Continue to next breakpoint
  - `Shift+F5` — Stop

---

## Post-Recording

- [ ] Add overlay text from the table above at each breakpoint pause
- [ ] Zoom in on Locals/Watch window moments
- [ ] Highlight the active debug line (already yellow in VS)
- [ ] Add lo-fi music (10-15% volume)
- [ ] Export 1080p 60fps
- [ ] Add YouTube chapters:
  - `0:00` Intro
  - `0:10` Code walkthrough — Method 1
  - `1:30` Code walkthrough — Method 2
  - `2:30` ParseFileName step-through
  - `3:00` SQL output
