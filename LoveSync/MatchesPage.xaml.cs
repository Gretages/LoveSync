using LoveSync.ViewModels;

namespace LoveSync;

public partial class MatchesPage : ContentPage
{
    public MatchesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MatchesViewModel vm)
            vm.LoadMatchesCommand.Execute(null);
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");
        return true;
    }
}
