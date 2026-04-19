# VoiceOfIslam.Api

Minimal API backend for VoiceOfIslam.

## How to Run

```
dotnet run --project VoiceOfIslam.Api/VoiceOfIslam.Api.csproj
```


## Endpoints
- `GET /` — Health check (returns a simple message)
- `GET /api/blobs` — List all audio streams (blobs)
- `GET /api/blobs/{id}` — Get a single audio stream (blob) by ID
- `POST /api/blobs` — Upload a new audio stream (demo, in-memory)

## Next Steps
- (Optional) Connect to a real database or storage
- Add authentication if needed
- Connect with Blazor WebAssembly frontend
