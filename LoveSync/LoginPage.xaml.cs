using LoveSync.ViewModels;

namespace LoveSync;

public partial class LoginPage : ContentPage
{
    // A konstruktorban megkapjuk a ViewModelt:
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }
}