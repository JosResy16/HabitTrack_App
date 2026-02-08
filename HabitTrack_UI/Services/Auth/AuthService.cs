using HabitTrack_UI.Utils;
using HabitTracker.Application.DTOs;
using HabitTrack_UI.Models.ErrorModels;
using HabitTrack_UI.Services.Api;
using Microsoft.AspNetCore.Components.Authorization;

namespace HabitTrack_UI.Services.Auth;
public class AuthService
{
    private readonly AuthApiClient _authApiClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly TokenStorageService _tokenStorageService;
    private readonly UserSession _userSession;
    private readonly ErrorService _errorService;

    public AuthService(
        AuthApiClient authApiClient,
        AuthenticationStateProvider authenticationStateProvider,
        TokenStorageService tokenStorageService,
        UserSession userSession,
        ErrorService errorService)
    {
        _authApiClient = authApiClient;
        _authenticationStateProvider = authenticationStateProvider;
        _tokenStorageService = tokenStorageService;
        _userSession = userSession;
        _errorService = errorService;
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var response = await _authApiClient.Login(request);

        await _tokenStorageService.SetTokens(
            response.AccessToken,
            response.RefreshToken);

        var principal = JwtUtils.TryCreatePrincipal(response.AccessToken);
        if (principal is null)
        {
            await LogoutAsync();
            _errorService.Raise(new AppError
            {
                Title = "Login failed",
                Message = "An invalid token was received from the server.",
                Type = ErrorType.Server,
            });
            return;
        }

        (_authenticationStateProvider as JwtAuthenticationStateProvider)?.NotifyUserAuthenticated(principal);

        await _userSession.Initialize(response.AccessToken);
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        await _authApiClient.Register(request);
    }

    public async Task LogoutAsync()
    {
        await _tokenStorageService.Clear();
        _userSession.Clear();

        (_authenticationStateProvider as JwtAuthenticationStateProvider)?.NotifyUserLogout();
    }

    public async Task<bool> TryRefreshTokenAsync()
    {
        var refreshToken = await _tokenStorageService.GetRefreshToken();
        if (string.IsNullOrEmpty(refreshToken))
            return false;

        var response = await _authApiClient.RefreshToken(new RefreshTokenRequestDTO { RefreshToken = refreshToken});

        await _tokenStorageService.SetTokens(
           response.AccessToken,
           response.RefreshToken);

        var principal = JwtUtils.TryCreatePrincipal(response.AccessToken);
        if (principal is null)
        {
            _errorService.Raise(new AppError
            {
                Title = "Unauthorized",
                Message = "Invalid token received from server",
                Type = ErrorType.Unauthorized,
            });

            return false;
        }

        (_authenticationStateProvider as JwtAuthenticationStateProvider)?.NotifyUserAuthenticated(principal);
        await _userSession.Initialize(response.AccessToken);

        return true;
    }

    public async Task InitializeAsync()
    {
        var token = await _tokenStorageService.GetToken();

        if (string.IsNullOrEmpty(token))
            return;

        var principal = JwtUtils.TryCreatePrincipal(token);

        if (principal == null)
        {
            await LogoutAsync();
            return;
        }

        (_authenticationStateProvider as JwtAuthenticationStateProvider)?.NotifyUserAuthenticated(principal);
        await _userSession.LoadAsync();
    }

}

