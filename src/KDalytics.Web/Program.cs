using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using KDalytics.Web;
using KDalytics.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// APIのベースURLを設定
var apiBaseAddress = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

// APIクライアントの登録
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<PlayerApiClient>();
builder.Services.AddScoped<MatchApiClient>();
builder.Services.AddScoped<PerformanceApiClient>();

await builder.Build().RunAsync();
