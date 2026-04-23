namespace LoveSync;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // 1. Szavazás
    private async void OnStartSwipingClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//CategoryPage");
    }

    // 2. Találatok
    private async void OnOpenMatchesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MatchesPage");
    }

    // 3. Üzenõfal
    private async void OnNotesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//NotesPage");
    }

    // 4. Bakancslista
    private async void OnBucketListClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//BucketListPage");
    }

    // 5. Profil
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }

    // 6. Naptár
    private async void OnCalendarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//CalendarPage");
    }
}