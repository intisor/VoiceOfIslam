using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VoiceOfIslam.Api.Data;
using VoiceOfIslam.Api.Services;
using VoiceOfIslam.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<BlobSasService>();
builder.Services.AddScoped<AudioService>();

builder.Services.AddOptions<BlobStorageOptions>()
    .BindConfiguration(BlobStorageOptions.SectionName);

var app = builder.Build();

// Serve Blazor WebAssembly static files and enable client-side routing
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapGet("/api/audio-streams", async (AudioService audioService) => await audioService.GetPastAudios());
app.MapGet("/api/audio-streams/live", async (AudioService audioService) => await audioService.GetLiveLecture());
app.MapGet("/api/audio-streams/recent/{count:int}", async (int count, AudioService audioService) => await audioService.GetRecentLectures(count));
app.MapGet("/api/audio-streams/{id:guid}", async (Guid id, AudioService audioService) =>
{
    var stream = await audioService.GetAudioStreamById(id);
    return stream is not null ? Results.Ok(stream) : Results.NotFound();
});
app.MapGet("/api/audio-streams/{id:guid}/playback-url", async (Guid id, AudioService audioService) =>
{
    var playbackUrl = await audioService.GetAuthorizedPlaybackUrl(id);
    return string.IsNullOrWhiteSpace(playbackUrl)
        ? Results.NotFound()
        : Results.Ok(new PlaybackUrlResponse { Url = playbackUrl });
});

app.MapFallbackToFile("index.html");

app.Run();
