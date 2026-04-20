using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoiceOfIslam.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container if needed
// builder.Services.AddCors();

var app = builder.Build();

// Serve Blazor WebAssembly static files and enable client-side routing
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

// API endpoints

// In-memory sample data for demonstration
var audioStreams = new List<AudioStream>
{
    new AudioStream {
        Id = Guid.NewGuid(),
        Title = "Sample Lecture 1",
        Description = "Introduction to Islam",
        BlobUrl = "https://example.com/audio1.mp3",
        CreatedAt = DateTime.UtcNow.AddDays(-2),
        Speaker = "Imam Ali",
        Duration = TimeSpan.FromMinutes(45)
    },
    new AudioStream {
        Id = Guid.NewGuid(),
        Title = "Sample Lecture 2",
        Description = "History of Prophets",
        BlobUrl = "https://example.com/audio2.mp3",
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        Speaker = "Imam Bilal",
        Duration = TimeSpan.FromMinutes(50)
    }
};

// Endpoint: List all audio streams (blobs)
app.MapGet("/api/blobs", () => audioStreams);

// Endpoint: Download a single audio stream (blob) by ID
app.MapGet("/api/blobs/{id}", (Guid id) =>
{
    var stream = audioStreams.FirstOrDefault(a => a.Id == id);
    return stream is not null ? Results.Ok(stream) : Results.NotFound();
});

// Endpoint: Upload a new audio stream (blob) (optional, demo only)
app.MapPost("/api/blobs", (AudioStream newStream) =>
{
    newStream.Id = Guid.NewGuid();
    newStream.CreatedAt = DateTime.UtcNow;
    audioStreams.Add(newStream);
    return Results.Created($"/api/blobs/{newStream.Id}", newStream);
});

app.Run();
