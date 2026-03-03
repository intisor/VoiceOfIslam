# Demo Script
# Update YOUR_ACCOUNT_NAME before recording

# ── METHOD 1: Microsoft Entra ID ─────────────────────────────────────────────
Write-Host "`n── METHOD 1: Microsoft Entra ID ──" -ForegroundColor Cyan

$env:AZURE_STORAGE_ACCOUNT_NAME    = "YOUR_ACCOUNT_NAME"   # <── change this
$env:AZURE_STORAGE_CONNECTION_STRING = ""
$env:AZURE_STORAGE_CONTAINER       = "archives"

dotnet run --project VoiceOfIslam.Tools

# ── METHOD 2: Connection String ───────────────────────────────────────────────
Write-Host "`n── METHOD 2: Connection String ──" -ForegroundColor Yellow

$env:AZURE_STORAGE_ACCOUNT_NAME    = ""
$env:AZURE_STORAGE_CONNECTION_STRING = "YOUR_CONNECTION_STRING"   # <── change this

dotnet run --project VoiceOfIslam.Tools
