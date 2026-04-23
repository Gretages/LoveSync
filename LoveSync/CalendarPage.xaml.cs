using LoveSync.ViewModels;
using Plugin.LocalNotification;

namespace LoveSync;

public partial class CalendarPage : ContentPage
{
    public CalendarPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. Engedély kérése (ha még nem volt)
        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        // 2. Adatok betöltése:
        if (BindingContext is CalendarViewModel vm)
        {
            await vm.LoadEvents();
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");
        return true;
    }
}