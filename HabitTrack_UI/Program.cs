using HabitTrack_UI;
using HabitTrack_UI.Services;
using HabitTrack_UI.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<TokenStorageService>();
builder.Services.AddScoped<AuthorizationHeaderHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7066/");
})
.AddHttpMessageHandler<AuthorizationHeaderHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));


builder.Services.AddScoped<HabitApiClient>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<CategoriesApiClient>();

await builder.Build().RunAsync();
