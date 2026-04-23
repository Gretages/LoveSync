using LoveSync.Models;
using LoveSync.ViewModels;

namespace LoveSync;

public partial class BucketListPage : ContentPage
{
    public BucketListPage()
    {
        InitializeComponent();
    }

    // Pipa ikon:
    private void OnItemCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is BucketItem item)
        {
            if (BindingContext is BucketViewModel vm)
            {
                // Átadjuk az elemet a ViewModelnek
                vm.ToggleItemCommand.Execute(item);
            }
        }
    }

    // Kuka ikon:
    private void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is BucketItem item)
        {
            if (BindingContext is BucketViewModel vm)
            {
                // Átadjuk az elemet a ViewModelnek törlésre
                vm.DeleteItemCommand.Execute(item);
            }
        }
    }
    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");

        return true;
    }
}