using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoveSync.Services;
using System.Threading.Tasks;
using System;

namespace LoveSync.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        string email;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        string statusMessage;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Kérlek töltsd ki a mezőket!";
                return;
            }

            try
            {
                StatusMessage = "Regisztráció folyamatban...";

                // 1. Regisztráció
                await _authService.RegisterAsync(Email, Password);

                // 2. Siker visszajelzés
                StatusMessage = "Sikeres regisztráció!";
                await Application.Current.MainPage.DisplayAlert("Siker", "Sikeres regisztráció! Most jelentkezz be.", "OK");

                // 3. Visszanavigálás a Login oldalra:
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Hiba: {ex.Message}";
            }
        }

        [RelayCommand]
        async Task GoBack()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}