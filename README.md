# VoiceOfIslam

## Blob metadata script
- Set `AZURE_STORAGE_CONNECTION_STRING` (and optionally `AZURE_STORAGE_CONTAINER`, `ARCHIVE_PREFIX`).
- Run `dotnet run --project VoiceOfIslam.Tools` to emit the SQL `INSERT` statement.
- Use command arguments to override env vars: `dotnet run --project VoiceOfIslam.Tools -- "<conn>" "archives" "optional-prefix"`.
