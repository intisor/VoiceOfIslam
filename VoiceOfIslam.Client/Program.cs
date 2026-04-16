using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using VoiceOfIslam.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<AudioPlayerService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<LectureService>();
builder.Services.AddScoped<SettingsService>();

await builder.Build().RunAsync();
