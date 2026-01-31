using HabitTrack_UI.Services.AppErrorService;
using System.Net.Http.Json;

namespace HabitTrack_UI.Services.Api;
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ErrorService _errorService;

    public ApiClient(
        HttpClient http,
        ErrorService errorService)
    {
        _http = http;
        _errorService = errorService;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendAsync<T>(request);
    }

    public async Task<T?> PostAsync<T>(string url, object? body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = body is null ? null : JsonContent.Create(body)
        };

        return await SendAsync<T>(request);
    }

    public async Task PutAsync(string url, object? body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = body is null ? null : JsonContent.Create(body)
        };

        await SendAsync<object>(request);
    }

    public async Task<bool> DeleteAsync(string url, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        await SendAsync<object>(request);
        return true;
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage request)
    {
        try
        {
            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = _errorService.FromStatusCode(response.StatusCode);
                _errorService.Raise(error);
                throw new AppException(error);
            }

            if (response.Content.Headers.ContentLength == 0)
                return default;

            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException ex)
        {
            var error = _errorService.FromHttp(ex);
            _errorService.Raise(error);
            throw new AppException(error);
        }
    }
}
