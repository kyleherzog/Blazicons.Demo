using Blazicons.Demo;
using Blazicons.Demo.Models;
using Blazor.Analytics;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorDownloadFile;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddGoogleAnalytics("GTM-W8DVLPZ");
builder.Services.AddSingleton<KeywordsManager>();

builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);

await builder.Build().RunAsync();
