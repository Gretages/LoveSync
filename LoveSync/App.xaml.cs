using LoveSync.Services;
using LoveSync.Models;
using Microsoft.Maui.Storage;

namespace LoveSync;

public partial class App : Application
{
    private readonly AuthService _authService;

    // Itt adjuk hozzá az AuthService-t is a paraméterekhez!
    public App(AppShell shell, AuthService authService)
    {
        InitializeComponent();

        _authService = authService;

        MainPage = shell;
    }

    // Ez fut le az alkalmazás indulásakor
    protected override async void OnStart()
    {
        base.OnStart();
        await CheckLogin();
    }

    private async Task CheckLogin()
    {
        string userId = Preferences.Get("CurrentUserId", string.Empty);

        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                var user = await _authService.GetUserAsync(userId);

                if (user != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        // Ha párban van, és nem a főoldalon vagyunk, akkor navigálunk
                        if (user.IsPaired)
                        {
                            // Csak akkor navigálunk, ha még nem ott vagyunk
                            if (Shell.Current.CurrentState?.Location.OriginalString != "//MainPage")
                            {
                                await Shell.Current.GoToAsync("//MainPage");
                            }
                        }
                        else
                        {
                            await Shell.Current.GoToAsync("//PairingPage");
                        }
                    });
                }
            }
            catch
            {
            }
        }
    }
}