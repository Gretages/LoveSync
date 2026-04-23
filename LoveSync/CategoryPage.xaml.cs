namespace LoveSync;

public partial class CategoryPage : ContentPage
{
    public CategoryPage()
    {
        InitializeComponent();
    }

    // Paraméter a Swipe oldalnak: ?type=Movie
    private async void OnMoviesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"SwipePage?type=Movie");
    }

    private async void OnFoodClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"SwipePage?type=Food");
    }

    private async void OnDateClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"SwipePage?type=Date");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}