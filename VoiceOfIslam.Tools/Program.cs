using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

// ── 1. Load config from env vars or CLI args ──────────────────────────────────
var options = ScriptOptions.FromEnvironment(args);
if (options.AccountName is null && options.ConnectionString is null)
{
	Console.Error.WriteLine("Set AZURE_STORAGE_ACCOUNT_NAME (recommended) or AZURE_STORAGE_CONNECTION_STRING.");
	return 1;
}

try
{
	// ── 2. Choose authentication method ──────────────────────────────────────
	//    AccountName set  →  Entra ID  (no secrets, works locally & in Azure)
	//    ConnectionString →  Account key  (fallback, less secure)
	var container = options.AccountName is { } account
		? new BlobContainerClient(new Uri($"https://{account}.blob.core.windows.net/{options.ContainerName}"), new DefaultAzureCredential())
		: new BlobContainerClient(options.ConnectionString, options.ContainerName);

	Console.WriteLine(options.AccountName is not null
		? $" Using Microsoft Entra ID (Account: {options.AccountName})"
		: " Using connection string");

	// ── 3. Stream blobs and build SQL rows ───────────────────────────────────
	List<string> rows = [];
	await foreach (var blob in container.GetBlobsAsync(prefix: options.BlobPrefix))
	{
		var (speaker, title) = ParseFileName(blob.Name, options.Speaker);
		var url = container.GetBlobClient(blob.Name).Uri.AbsoluteUri;
		rows.Add($"    (NEWID(), '{Esc(title)}', '{Esc(options.Description)}', '{Esc(url)}', GETUTCDATE(), '{Esc(speaker)}', 0, '{options.Duration}')");
	}

	// ── 4. Build SQL using raw string literal (C# 11+) ───────────────────────
	var sql = rows.Count == 0 ? "-- No blobs found." : $"""
		INSERT INTO [dbo].[AudioStreams] ([Id], [Title], [Description], [BlobUrl], [CreatedAt], [Speaker], [IsLive], [Duration]) VALUES
		{string.Join(",\n", rows)};
		""";

	// ── 5. Write to file ──────────────────────────────────────────────────────
	var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "AudioStreams_Insert.sql");
	await File.WriteAllTextAsync(outputPath, sql);
	Console.WriteLine($"✅ Done → {outputPath}");
	return 0;
}
catch (RequestFailedException ex) { Console.Error.WriteLine($"Azure error: {ex.Message}"); return 2; }
catch (Exception ex)              { Console.Error.WriteLine($"Error: {ex.Message}");        return 99; }

// ── Helpers ───────────────────────────────────────────────────────────────────

static string Esc(string v) => v.Replace("'", "''");

// ── 6. Parse speaker + title from blob filename ───────────────────────────────
static (string Speaker, string Title) ParseFileName(string blobName, string defaultSpeaker)
{
	string[] honorifics = ["Muallim", "Prof.", "Prof", "Sheikh", "Barr.", "Barr", "Maulvi", "Amir", "Dr.", "Dr", "Ustadh", "Imam"];

	var name = Path.GetFileNameWithoutExtension(blobName);
	if (string.IsNullOrWhiteSpace(name)) return (defaultSpeaker, blobName);

	// Strip brand noise from filename
	var clean = name
		.Replace("Ohun Islam Lagos State", "", StringComparison.OrdinalIgnoreCase)
		.Replace("Ohun Islam Lagos",       "", StringComparison.OrdinalIgnoreCase)
		.Replace("Ohun Islam",             "", StringComparison.OrdinalIgnoreCase)
		.Trim().Trim('-', '_', ' ');

	if (string.IsNullOrWhiteSpace(clean)) return (defaultSpeaker, name);

	// Episode pattern: look for speaker before or after "Episode"
	var epi = clean.IndexOf("Episode", StringComparison.OrdinalIgnoreCase);
	if (epi >= 0)
	{
		var before = clean[..epi].Trim('-', '_', ' ');
		var after  = clean[epi..];
		var h = honorifics.FirstOrDefault(t => before.Contains(t, StringComparison.OrdinalIgnoreCase))
			 ?? honorifics.FirstOrDefault(t => after.Contains(t, StringComparison.OrdinalIgnoreCase));

		if (h is null) return (defaultSpeaker, clean);

		var inBefore = before.Contains(h, StringComparison.OrdinalIgnoreCase);
		var src = inBefore ? before : after;
		int idx = src.IndexOf(h, StringComparison.OrdinalIgnoreCase);
		var speaker = src[idx..].Trim();
		var topic = inBefore && idx > 0 ? before[..idx].Trim('-', '_', ' ') : "";
		var title = string.IsNullOrWhiteSpace(topic) ? after.Trim() : $"{topic} {after}".Trim();
		return (speaker, inBefore ? title : $"{before} {after[..idx]}".Trim());
	}

	// No episode: honorific marks where speaker name starts
	var found = honorifics.FirstOrDefault(t => clean.Contains(t, StringComparison.OrdinalIgnoreCase));
	if (found is not null)
	{
		int idx = clean.IndexOf(found, StringComparison.OrdinalIgnoreCase);
		return (clean[idx..].Trim(), idx > 0 ? clean[..idx].Trim('-', '_', ' ') : "General Lecture");
	}

	// Last resort: split on final dash/underscore
	var delim = Math.Max(clean.LastIndexOf('-'), clean.LastIndexOf('_'));
	if (delim > 0 && delim < clean.Length - 1)
	{
		var after = clean[(delim + 1)..].Trim();
		if (char.IsUpper(after[0])) return (after, clean[..delim].Trim());
	}

	return (defaultSpeaker, clean);
}

// ── 7. Config: reads env vars first, CLI args override ───────────────────────
internal sealed record ScriptOptions(
	string? AccountName, string? ConnectionString,
	string ContainerName, string Description,
	string Speaker, string Duration, string? BlobPrefix)
{
	public static ScriptOptions FromEnvironment(string[] args)
	{
		static string? Env(string key) => Environment.GetEnvironmentVariable(key);

		var accountName = Env("AZURE_STORAGE_ACCOUNT_NAME");
		var connStr     = Env("AZURE_STORAGE_CONNECTION_STRING");
		var container   = Env("AZURE_STORAGE_CONTAINER") ?? "archives";
		var description = Env("ARCHIVE_DESCRIPTION")     ?? "Lagos State";
		var speaker     = Env("ARCHIVE_SPEAKER")         ?? "Unknown Speaker";
		var duration    = Env("ARCHIVE_DURATION")        ?? "00:00:00";
		var prefix      = Env("ARCHIVE_PREFIX");

		// CLI args override env vars
		if (args is [var a0, ..] && !string.IsNullOrWhiteSpace(a0))
		{
			if (a0.Contains("AccountName=", StringComparison.OrdinalIgnoreCase)) connStr = a0;
			else accountName = a0;
		}
		if (args is [_, var a1, ..] && !string.IsNullOrWhiteSpace(a1)) container = a1;
		if (args is [_, _, var a2, ..] && !string.IsNullOrWhiteSpace(a2)) prefix = a2;

		return new(accountName, connStr, container, description, speaker, duration, prefix);
	}
}
