namespace HabitTrack_UI.Services;
public class UserSession
{
    private readonly AuthApiClient _authApiClient;

    public event Action? OnChange;

    public UserSession(AuthApiClient authApiClient)
    {
        _authApiClient = authApiClient;
    }

    public bool IsLoggedIn { get; private set; }
    public string? UserName { get; private set; }
    public string? Email { get; private set; }
    public string? UserId { get; private set; }
    public string? Role { get; private set; }

    public async Task LoadAsync()
    {
        try
        {
            var me = await _authApiClient.Me();

            IsLoggedIn = true;
            UserName = me.UserName;
            Email = me.Email;
            UserId = me.Id;
            Role = me.Role;

            NotifyStateChanged();
        }
        catch
        {
            Clear();
        }
    }

    public async Task Initialize(string jwt)
    {
        await LoadAsync();
    }

    public void Clear()
    {
        IsLoggedIn = false;
        UserName = null;
        Email = null;
        UserId = null;
        Role = null;

        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }

}

