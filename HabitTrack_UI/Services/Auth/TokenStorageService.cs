using Microsoft.JSInterop;

namespace HabitTrack_UI.Services.Auth;
public class TokenStorageService
{
    private const string TokenKey = "auth_token";
    private const string RefreshTokenKey = "refresh_token";

    private readonly IJSRuntime _js;

    public TokenStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SetTokens(string token, string refreshToken)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
    }

    public async Task<string?> GetToken()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    public async Task<string?> GetRefreshToken()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
    }

    public async Task Clear()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
    }
}
