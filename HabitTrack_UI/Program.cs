using HabitTrack_UI;
using HabitTrack_UI.Services;
using HabitTrack_UI.Services.Api;
using HabitTrack_UI.Services.Auth;
using HabitTrack_UI.Services.Habits;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Headers;

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
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api");

    client.DefaultRequestHeaders.Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

    return client;
});


builder.Services.AddScoped<ErrorService>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<HabitApiClient>();
builder.Services.AddScoped<HabitsService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<HabitStatsService>();
builder.Services.AddScoped<StatsApiClient>();
builder.Services.AddScoped<CategoriesApiClient>();


await builder.Build().RunAsync();
