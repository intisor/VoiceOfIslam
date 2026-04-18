using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using VoiceOfIslam.Components;
using VoiceOfIslam.Data;
using VoiceOfIslam.Services;
using VoiceOfIslam.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddScoped<AudioPlayerService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<AudioService>();
builder.Services.AddScoped<BlobSasService>();
builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(sp.GetRequiredService<NavigationManager>().BaseUri)
});
builder.Services.AddScoped<LectureService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddOptions<BlobStorageOptions>()
	.BindConfiguration(BlobStorageOptions.SectionName);

// Add DbContext configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Using AddDbContextFactory for Blazor performance and thread-safety
builder.Services.AddDbContextFactory<AppDbContext>(options =>
	options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();
	// .AddAdditionalAssemblies(typeof(VoiceOfIslam.Client.Pages.Home).Assembly);

app.MapGet("/api/audio-streams", async (AudioService audioService) => await audioService.GetPastAudios());
app.MapGet("/api/audio-streams/live", async (AudioService audioService) => await audioService.GetLiveLecture());
app.MapGet("/api/audio-streams/recent/{count:int}", async (int count, AudioService audioService) => await audioService.GetRecentLectures(count));
app.MapGet("/api/audio-streams/{id:guid}/playback-url", async Task<Results<Ok<PlaybackUrlResponse>, NotFound>> (Guid id, AudioService audioService) =>
{
	var playbackUrl = await audioService.GetAuthorizedPlaybackUrl(id);
	return string.IsNullOrWhiteSpace(playbackUrl)
		? TypedResults.NotFound()
		: TypedResults.Ok(new PlaybackUrlResponse { Url = playbackUrl });
});

app.Run();
