using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoveSync.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace LoveSync.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        string email;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        bool isBusy;

        [ObservableProperty]
        string statusMessage;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        async Task Login()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Kérlek tölts ki minden mezőt!";
                return;
            }

            IsBusy = true;
            StatusMessage = "Bejelentkezés...";

            try
            {
                string userId = await _authService.LoginAsync(Email, Password);
                var user = await _authService.GetUserAsync(userId);

                StatusMessage = "Sikeres belépés!";

                if (user != null && user.IsPaired)
                {
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await Shell.Current.GoToAsync("//PairingPage");
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

        [RelayCommand]
        async Task GoToRegister()
        {
            await Shell.Current.GoToAsync("//RegisterPage");
        }
    }
}