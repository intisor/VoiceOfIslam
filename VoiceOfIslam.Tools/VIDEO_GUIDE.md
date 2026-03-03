# Video Walkthrough Guide
## Azure Blob Storage Authentication — Step-Into Debugging

> No narration. Every overlay IS the narration.
> Copy each overlay block exactly into your video editor as on-screen text.

---

## Before You Press F5

1. Select debug profile from the VS dropdown
2. Press F5 — execution stops at each breakpoint
3. F10 = step over · F11 = step into · F5 = jump to next breakpoint
4. Keep Watch window open with all variables loaded (see bottom of this file)

---

## SECTION 1 — Line 7 · Load Config

**Breakpoint on:** `var options = ScriptOptions.FromEnvironment(args);`

**Press F11 to step inside FromEnvironment**

---

### Overlay 1A — when you land on the local Env() function

```
This tool reads all its settings from environment variables.
Instead of calling Environment.GetEnvironmentVariable() seven times,
a local static function called Env() wraps it in a single line.
This is a C# 13 idiom — keep your code DRY without creating a whole helper class.
```

---

### Overlay 1B — when you step through the variable assignments

```
Each environment variable has a fallback default using the ?? operator.
If AZURE_STORAGE_CONTAINER is not set, it defaults to "archives".
If ARCHIVE_SPEAKER is not set, it defaults to "Unknown Speaker".
No config file needed. No appsettings.json. Just environment variables.
```

---

### Overlay 1C — when you land on the list pattern args check

```
This is a C# 11 list pattern:  args is [var a0, ..]
It means: "if the args array has at least one element, bind the first to a0".
This replaces the old  args.Length > 0 && args[0] != null  pattern.
CLI arguments always override environment variables — args win.
```

---

### Overlay 1D — hover options after returning, expand in Locals

```
The ScriptOptions record is now fully loaded.
Expand it in the Locals window — you can see all 7 properties at once.
AccountName is set for Method 1. ConnectionString is null.
For Method 2, it is the opposite — AccountName is null, ConnectionString has a value.
```

---

## SECTION 2 — Line 19 · Authentication Decision

**Breakpoint on:** `var container = options.AccountName is { } account`

---

### Overlay 2A — before stepping (show the ternary)

```
This single ternary is where the two authentication methods diverge.
The condition  options.AccountName is { } account  is a property pattern.
It means: "if AccountName is not null, extract its value into a new variable called account".
If that condition is true — Method 1 runs. If false — Method 2 runs.
The entire auth decision lives in three lines.
```

---

### Overlay 2B — Method 1 take · when the top branch executes

```
AccountName was set, so Method 1 runs — Microsoft Entra ID.
A URI is built:  https://{accountName}.blob.core.windows.net/{containerName}
Then DefaultAzureCredential is passed as the credential object.
DefaultAzureCredential automatically tries a chain of identity providers:
Azure CLI (az login) locally, Managed Identity when deployed to Azure.
There are no secrets anywhere — no keys, no passwords, no connection strings.
```

---

### Overlay 2C — Method 2 take · when the bottom branch executes

```
AccountName is null, so Method 2 runs — Connection String authentication.
The connection string already contains the account name, the endpoint, AND the account key.
It is passed directly to BlobContainerClient — no separate credential object needed.
This is why connection strings are less secure:
the key is a plain string sitting in your environment variable.
Anyone who reads it has full access to the storage account.
```

---

### Overlay 2D — hover container after either branch

```
Hover over the container variable.
Expand it and look at the Uri property.
Both methods created a BlobContainerClient pointing to the same container.
The authentication mechanism is different — the destination is identical.
This is the power of the Azure SDK abstraction.
```

---

## SECTION 3 — Lines 29–34 · Stream Blobs

**Breakpoint on:** `await foreach (var blob in container.GetBlobsAsync(prefix: options.BlobPrefix))`

---

### Overlay 3A — on the await foreach line

```
GetBlobsAsync returns an IAsyncEnumerable — an async stream.
It does not load all blobs into memory at once.
Each blob is yielded one at a time as it arrives from Azure.
The prefix parameter filters by folder path — only blobs under that prefix are returned.
If BlobPrefix is null, every blob in the container is processed.
```

---

### Overlay 3B — on the ParseFileName call · pin blob.Name

```
Look at blob.Name in the Watch window.
This is the raw filename as stored in Azure Blob Storage.
It contains brand noise, speaker names, episode numbers, and honorifics all mixed together.
The next step — ParseFileName — will break this into a clean speaker and title.
Press F11 to step inside and watch the transformation happen.
```

---

### Overlay 3C — on the rows.Add line · watch rows.Count increment

```
Each blob produces one SQL row string.
The Esc() helper replaces single quotes with double single quotes
to prevent SQL injection in the literal values.
Watch the rows.Count in the Watch window — it increments with every blob.
Every iteration = one audio file = one database record.
```

---

## SECTION 4 — Lines 37–40 · Build SQL

**Breakpoint on:** `var sql = rows.Count == 0 ? "-- No blobs found." : $"""`

---

### Overlay 4A — show the raw string literal

```
This is a C# 11 raw interpolated string literal — $""" ... """
Before this feature you would need a StringBuilder with multiple AppendLine calls.
Now the entire INSERT statement is written as plain readable text
with {string.Join()} embedded directly inside it.
The indentation of the closing """ tells the compiler how much whitespace to strip,
so the output SQL has no unwanted leading spaces.
```

---

### Overlay 4B — hover sql · expand the string value

```
Hover over the sql variable and expand it.
You can see the complete INSERT statement — fully formed, ready to execute.
Every blob that was streamed from Azure is now a row in this SQL.
Copy this into SSMS and run it — your AudioStreams table is populated.
```

---

## SECTION 5 — Lines 43–45 · Write File

**Breakpoint on:** `var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "AudioStreams_Insert.sql");`

---

### Overlay 5A — pin outputPath

```
The output file is written to the current working directory.
During debugging that is the project folder.
Pin the outputPath variable — you can see the exact full path on screen.
File.WriteAllTextAsync opens, writes, and closes the file in a single async call.
After this line finishes, open the file — the SQL is ready to paste into SSMS.
```

---

## SECTION 6 — Lines 56–109 · ParseFileName Step-Through

**Press F11 on Line 31 to step inside this function**

---

### Overlay 6A — entering ParseFileName · pin blobName

```
We are now inside ParseFileName.
The raw blob name comes in through the blobName parameter.
Pin it — you can see something like:
"Ohun Islam Lagos - Tafsir Episode 5 - Sheikh Musa.mp3"
The goal of this function is to extract two things from that string:
the speaker name and the lecture title.
```

---

### Overlay 6B — after the .Replace() chain · pin clean

```
The first thing the function does is strip brand noise.
Three Replace() calls remove variations of the brand name from the filename.
Pin the clean variable — compare it to blobName.
The brand noise is gone. What remains is just the content:
"Tafsir Episode 5 - Sheikh Musa"
This is the string we actually parse.
```

---

### Overlay 6C — after epi = IndexOf("Episode") · pin epi

```
Now the function checks whether the filename contains the word "Episode".
IndexOf returns the character position if found, or -1 if not.
Pin epi.
If epi is -1 — no episode in the name — the code skips to the honorific check.
If epi is 0 or greater — an episode was found — the code splits the string
into "before Episode" and "after Episode" and searches both parts for a speaker.
```

---

### Overlay 6D — episode path · pin before, after, h

```
The string is split at the Episode position.
"before" is everything to the left — this is likely the topic name or speaker.
"after" is everything to the right including "Episode 5 - Sheikh Musa".
The function checks "before" first for a speaker honorific.
If not found there, it checks "after".
The ?? operator means: try the first, fall back to the second.
Pin h — you can see which honorific was matched, for example "Sheikh".
```

---

### Overlay 6E — honorific path · pin found, idx

```
When there is no Episode in the filename, the function looks for an honorific directly.
The honorifics array contains titles like Sheikh, Dr., Prof, Imam, Ustadh, Barr.
FirstOrDefault returns the first match, or null if none found.
Once a match is found, its index in the string tells us where the speaker name starts.
Everything from that index to the end = speaker.
Everything before it = the lecture title.
```

---

### Overlay 6F — fallback · pin delim

```
If no honorific was found at all — this is the last resort.
The function finds the last dash or underscore in the cleaned filename.
If the word after that delimiter starts with a capital letter,
it is assumed to be a speaker name.
This handles filenames that use a consistent naming convention
but do not include a formal title.
```

---

## SECTION 7 — Lines 112–139 · ScriptOptions Record

**After stepping out of FromEnvironment — hover options in Locals, expand it**

---

### Overlay 7A — show the record declaration

```
ScriptOptions is a positional record — introduced in C# 9.
Declaring properties in the primary constructor like this
automatically generates immutable init-only properties.
No get; set; blocks. No backing fields. No constructor body.
sealed prevents inheritance — this type is not designed to be extended.
internal means it is private to this project.
string? with a nullable marker on AccountName and ConnectionString
means either one can legitimately be null — but not both.
```

---

### Overlay 7B — expand options in the Locals window

```
Expand the options object in the Locals window.
Every property is visible — AccountName, ConnectionString, ContainerName,
Description, Speaker, Duration, BlobPrefix.
For Method 1: AccountName has a value, ConnectionString is null.
For Method 2: AccountName is null, ConnectionString has a value.
The rest of the properties come from environment variables with sensible defaults.
This is the full picture of what the tool knows before it connects to Azure.
```

---

## Watch Window — Add These Before Pressing F5

Open **Debug → Windows → Watch → Watch 1** and type each expression:

```
options.AccountName
options.ConnectionString
blob.Name
speaker
title
clean
epi
rows.Count
sql
outputPath
```

These update live as you step.
Keep the Watch window docked and visible the entire recording.
It replaces narration — the viewer reads the values directly.

---

## Two Takes Side by Side

| | Method 1 — Entra ID | Method 2 — Connection String |
|---|---|---|
| Profile | Method 1 - Entra ID | Method 2 - Connection String |
| Line 19 | Top branch executes | Bottom branch executes |
| account pin | Shows storage account name | Not bound — condition was false |
| ConnectionString pin | null | Shows full key string |
| Security message | ✅ Using Microsoft Entra ID | ⚠️ Using connection string |
| Key overlay | "No secrets. Identity resolves from az login." | "Account key is visible right here in the string." |
