using LoveSync.ViewModels;

namespace LoveSync;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Frissítjük az adatokat:
        if (BindingContext is ProfileViewModel vm)
        {
            // Meghívjuk a LoadProfileDataCommand-ot
            vm.LoadProfileDataCommand.Execute(null);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        // Átirányítjuk a felhasználót a Kezdõlapra:
        Shell.Current.GoToAsync("//MainPage");

        return true;
    }
}