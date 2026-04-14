using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VoiceOfIslam.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<AudioPlayerService>();
builder.Services.AddScoped<MenuService>();

await builder.Build().RunAsync();
