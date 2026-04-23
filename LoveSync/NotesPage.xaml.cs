using LoveSync.ViewModels;
using Plugin.LocalNotification;
using System.Collections.Specialized;

namespace LoveSync;

public partial class NotesPage : ContentPage
{
    public NotesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        if (BindingContext is NotesViewModel vm)
        {
            // 1. Csak jelezzük a VM-nek, hogy itt vagyunk
            await vm.AppearingCommand.ExecuteAsync(null);

            // 2. Görgetés az aljára:
            ScrollToBottom(vm, false);

            // 3. Ha jön üzenet, akkor is az aljára:
            vm.Notes.CollectionChanged += Notes_CollectionChanged;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Leiratkozás a memóriaszivárgás elkerülése végett
        if (BindingContext is NotesViewModel vm)
        {
            vm.Notes.CollectionChanged -= Notes_CollectionChanged;
        }
    }

    // Ha új üzenet jön, görgessünk le
    private void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (BindingContext is NotesViewModel vm)
        {
            // Csak ha új elem jött (NewItems), görgetünk animálva
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ScrollToBottom(vm, true);
            }
        }
    }

    private void ScrollToBottom(NotesViewModel vm, bool animate)
    {
        // Csak akkor tudunk görgetni, ha van elem
        if (vm.Notes.Count > 0)
        {
            // Kis késleltetés kell a MAUI-nak, hogy rendereljen
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                try
                {
                    // Az utolsó elemre görgetünk
                    MessagesList.ScrollTo(vm.Notes.Last(), null, ScrollToPosition.End, animate);
                }
                catch { } // Ha véletlenül gyorsan elnavigáltál, ne fagyjon le
            });
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");
        return true;
    }
}