using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

// Generates SQL insert statements for Azure Blob audio archives.
var options = ScriptOptions.FromEnvironment(args);
if (!options.TryValidate(out var validationMessage))
{
	Console.Error.WriteLine(validationMessage);
	return 1;
}

try
{
	var sql = await BlobSqlGenerator.GenerateAsync(options);
	
	// Write to file
	var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "AudioStreams_Insert.sql");
	await File.WriteAllTextAsync(outputPath, sql);
	
	Console.WriteLine($"? SQL script generated successfully!");
	Console.WriteLine($"?? Location: {outputPath}");
	Console.WriteLine($"?? Ready to paste into SSMS");
	return 0;
}
catch (RequestFailedException ex)
{
	Console.Error.WriteLine($"Azure request failed: {ex.Message}");
	return 2;
}
catch (Exception ex)
{
	Console.Error.WriteLine($"Unexpected error: {ex.Message}");
	return 99;
}

internal static class BlobSqlGenerator
{
	public static async Task<string> GenerateAsync(ScriptOptions options)
	{
		var containerClient = new BlobContainerClient(options.ConnectionString!, options.ContainerName);
		var rows = new List<string>();
		await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(states: BlobStates.None, traits: BlobTraits.None, prefix: options.BlobPrefix))
		{
			var data = ParseFileName(blobItem.Name, options.Speaker);
			var blobClient = containerClient.GetBlobClient(blobItem.Name);
			var url = blobClient.Uri.AbsoluteUri;
			var row =
				$"    (NEWID(), '{EscapeSqlLiteral(data.Title)}', '{EscapeSqlLiteral(options.Description)}', '{EscapeSqlLiteral(url)}', GETUTCDATE(), '{EscapeSqlLiteral(data.Speaker)}', 0, '{options.Duration}')";
			rows.Add(row);
		}

		if (rows.Count == 0)
		{
			return "-- No blobs found for the provided container/prefix.";
		}

		var sqlBuilder = new StringBuilder();
		sqlBuilder.AppendLine("INSERT INTO [dbo].[AudioStreams] ([Id], [Title], [Description], [BlobUrl], [CreatedAt], [Speaker], [IsLive], [Duration]) VALUES");
		sqlBuilder.AppendLine(string.Join(",\n", rows));
		sqlBuilder.Append(';');
		return sqlBuilder.ToString();
	}

	private static (string Speaker, string Title) ParseFileName(string blobName, string defaultSpeaker)
	{
		var fileName = Path.GetFileNameWithoutExtension(blobName);
		if (string.IsNullOrWhiteSpace(fileName))
		{
			return (defaultSpeaker, blobName);
		}

		// Step 1: Strip the "Brand" noise
		var clean = fileName
			.Replace("Ohun Islam Lagos State", "", StringComparison.OrdinalIgnoreCase)
			.Replace("Ohun Islam Lagos", "", StringComparison.OrdinalIgnoreCase)
			.Replace("Ohun Islam", "", StringComparison.OrdinalIgnoreCase)
			.Trim()
			.Trim('-', '_', ' ');

		if (string.IsNullOrWhiteSpace(clean))
		{
			return (defaultSpeaker, fileName);
		}

		string finalTitle = clean;
		string finalSpeaker = defaultSpeaker;
		string[] titles = { "Muallim", "Prof.", "Prof", "Sheikh", "Barr.", "Barr", "Maulvi", "Amir", "Dr.", "Dr", "Ustadh", "Imam" };

		// Step 2: THE EPISODE RULE - Check for Episode FIRST
		var episodeIndex = clean.IndexOf("Episode", StringComparison.OrdinalIgnoreCase);
		if (episodeIndex >= 0)
		{
			// First, check BEFORE Episode for speaker
			var beforeEpisode = clean[..episodeIndex].Trim('-', '_', ' ');
			var foundTitleBefore = titles.FirstOrDefault(t => beforeEpisode.Contains(t, StringComparison.OrdinalIgnoreCase));

			if (foundTitleBefore != null)
			{
				// Speaker found before Episode
				int titleIdx = beforeEpisode.IndexOf(foundTitleBefore, StringComparison.OrdinalIgnoreCase);
				finalSpeaker = beforeEpisode[titleIdx..].Trim();
				var topicPart = titleIdx > 0 ? beforeEpisode[..titleIdx].Trim('-', '_', ' ') : "";
				finalTitle = string.IsNullOrWhiteSpace(topicPart) 
					? clean[episodeIndex..].Trim() 
					: $"{topicPart} {clean[episodeIndex..]}".Trim();
			}
			else
			{
				// No speaker before Episode, check AFTER "Episode X-" pattern
				var afterEpisode = clean[episodeIndex..];
				var foundTitleAfter = titles.FirstOrDefault(t => afterEpisode.Contains(t, StringComparison.OrdinalIgnoreCase));
				
				if (foundTitleAfter != null)
				{
					// Speaker found after Episode
					int titleIdx = afterEpisode.IndexOf(foundTitleAfter, StringComparison.OrdinalIgnoreCase);
					finalSpeaker = afterEpisode[titleIdx..].Trim();
					// Remove speaker from the episode part
					var episodePart = afterEpisode[..titleIdx].Trim('-', '_', ' ');
					finalTitle = $"{beforeEpisode} {episodePart}".Trim();
				}
				else
				{
					// No speaker found anywhere
					finalSpeaker = defaultSpeaker;
					finalTitle = clean;
				}
			}
		}
		else
		{
			// Step 3: No Episode - standard title detection
			var foundTitle = titles.FirstOrDefault(t => clean.Contains(t, StringComparison.OrdinalIgnoreCase));

			if (foundTitle != null)
			{
				int titleIdx = clean.IndexOf(foundTitle, StringComparison.OrdinalIgnoreCase);
				finalSpeaker = clean[titleIdx..].Trim();
				finalTitle = titleIdx > 0 ? clean[..titleIdx].Trim('-', '_', ' ') : "General Lecture";
			}
			else
			{
				// Fallback: try last delimiter
				var lastDash = clean.LastIndexOf('-');
				var lastUnderscore = clean.LastIndexOf('_');
				var lastDelimiter = Math.Max(lastDash, lastUnderscore);

				if (lastDelimiter > 0 && lastDelimiter < clean.Length - 1)
				{
					var afterDelimiter = clean[(lastDelimiter + 1)..].Trim();
					if (!string.IsNullOrWhiteSpace(afterDelimiter) && char.IsUpper(afterDelimiter[0]))
					{
						finalSpeaker = afterDelimiter;
						finalTitle = clean[..lastDelimiter].Trim();
					}
				}
			}
		}

		// Final cleanup
		finalSpeaker = finalSpeaker.Trim('-', '_', ' ');
		finalTitle = finalTitle.Trim('-', '_', ' ');

		return (finalSpeaker, finalTitle);
	}

	private static string EscapeSqlLiteral(string value)
		=> value.Replace("'", "''");
}

internal sealed record ScriptOptions(
	string? ConnectionString,
	string ContainerName,
	string Description,
	string Speaker,
	string Duration,
	string? BlobPrefix)
{
	public static ScriptOptions FromEnvironment(string[] args)
	{
		var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
		var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER") ?? "archives";
		var description = Environment.GetEnvironmentVariable("ARCHIVE_DESCRIPTION") ?? "Lagos State";
		var speaker = Environment.GetEnvironmentVariable("ARCHIVE_SPEAKER") ?? "Unknown Speaker";
		var duration = Environment.GetEnvironmentVariable("ARCHIVE_DURATION") ?? "00:00:00";
		var prefix = Environment.GetEnvironmentVariable("ARCHIVE_PREFIX");

		if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
		{
			connectionString = args[0];
		}
		if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
		{
			containerName = args[1];
		}
		if (args.Length > 2 && !string.IsNullOrWhiteSpace(args[2]))
		{
			prefix = args[2];
		}

		return new ScriptOptions(connectionString, containerName, description, speaker, duration, prefix);
	}

	public bool TryValidate(out string message)
	{
		if (string.IsNullOrWhiteSpace(ConnectionString))
		{
			message = "Set AZURE_STORAGE_CONNECTION_STRING or pass it as the first argument.";
			return false;
		}

		message = string.Empty;
		return true;
	}
}
