using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoveSync.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace LoveSync.ViewModels
{
    public partial class PairingViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private IDispatcherTimer _timer;

        [ObservableProperty]
        string generatedCode;

        [ObservableProperty]
        string inputCode;

        [ObservableProperty]
        string statusMessage;

        [ObservableProperty]
        bool isBusy;

        public PairingViewModel(AuthService authService)
        {
            _authService = authService;

            // 2 mp időzítő:
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += async (s, e) => await CheckIfPaired();
            _timer.Start();
        }

        private async Task CheckIfPaired()
        {
            if (IsBusy) return;

            try
            {
                string myId = Preferences.Get("CurrentUserId", "");
                if (string.IsNullOrEmpty(myId)) return;

                // Lekérjük a friss adatainkat
                var user = await _authService.GetUserAsync(myId);

                // Ha az adatbázisban a státuszunk átváltott "IsPaired = true"-ra
                if (user != null && user.IsPaired)
                {
                    _timer.Stop();

                    // Átirányítás a Főoldalra
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.GoToAsync("//MainPage");
                    });
                }
            }
            catch (Exception)
            {
            }
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }

        [RelayCommand]
        async Task GenerateCode()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                StatusMessage = "Kód generálása...";
                string code = await _authService.GeneratePairingCodeAsync();
                GeneratedCode = code;
                StatusMessage = "Kód aktív! Add meg a párodnak, és várj...";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Hiba: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task JoinWithCode()
        {
            if (string.IsNullOrWhiteSpace(InputCode) || IsBusy) return;
            IsBusy = true;

            try
            {
                StatusMessage = "Csatlakozás...";

                // Megpróbáljuk a párosítást a beírt kóddal
                bool success = await _authService.PairWithUserAsync(InputCode);

                if (success)
                {
                    StatusMessage = "Sikeres párosítás!";
                    _timer.Stop();

                    // Főoldalra navigálás:
                    await Shell.Current.GoToAsync("//MainPage");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Hiba: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}