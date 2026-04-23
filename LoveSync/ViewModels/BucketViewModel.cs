using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoveSync.Models;
using LoveSync.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LoveSync.ViewModels
{
    public partial class BucketViewModel : ObservableObject
    {
        private readonly BucketService _bucketService = new BucketService();

        public ObservableCollection<BucketItem> Items { get; set; } = new();

        [ObservableProperty]
        string newItemText;

        [ObservableProperty]
        bool isBusy;

        public BucketViewModel()
        {
            LoadItemsCommand.Execute(null);
        }

        private bool _isInitialized = false;

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            // Ez ugyanaz, mint amit a Command hív, csak nem Commandból hívjuk
            await LoadItems();
        }

        [RelayCommand]
        async Task LoadItems()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var list = await _bucketService.GetBucketListAsync();
                Items.Clear();
                foreach (var item in list)
                {
                    Items.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task AddItem()
        {
            if (string.IsNullOrWhiteSpace(NewItemText)) return;

            await _bucketService.AddItemAsync(NewItemText);
            NewItemText = string.Empty;
            await LoadItems();
        }

        [RelayCommand]
        async Task DeleteItem(BucketItem item)
        {
            if (item == null) return;

            bool confirm = await Microsoft.Maui.Controls.Shell.Current.DisplayAlert("Törlés", "Biztos törlöd?", "Igen", "Nem");
            if (confirm)
            {
                await _bucketService.DeleteItemAsync(item);
                Items.Remove(item);
            }
        }

        // CheckBox event:
        [RelayCommand]
        async Task ToggleItem(BucketItem item)
        {
            if (item == null) return;
            //Ab mentés:
            await _bucketService.UpdateItemAsync(item);
        }
    }
}