using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoveSync.Models;
using LoveSync.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LoveSync.ViewModels
{
    [QueryProperty(nameof(CategoryType), "type")]
    public partial class SwipeViewModel : ObservableObject
    {
        private readonly AuthService _authService = new AuthService();
        private readonly MovieService _movieService = new MovieService();
        private readonly IdeaService _ideaService = new IdeaService();

        private int _currentIndex = 0;
        private CancellationTokenSource _loadCts;

        [ObservableProperty]
        private string categoryType;

        public ObservableCollection<Idea> Ideas { get; } = new();

        [ObservableProperty]
        private Idea currentIdea;

        [ObservableProperty]
        private ImageSource currentIdeaImage = ImageSource.FromFile("placeholder.png");

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCardsVisible))]
        private bool isFinished;

        [ObservableProperty]
        private bool isSwipeEnabled = true;

        public bool IsCardsVisible => !IsFinished;

        partial void OnCategoryTypeChanged(string value)
        {
            InitializeData(value);
        }

        partial void OnCurrentIdeaChanged(Idea value)
        {
            CurrentIdeaImage = BuildImageSource(value);
        }

        public SwipeViewModel() { }

        private async void InitializeData(string type)
        {
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            var ct = _loadCts.Token;

            IsFinished = true;
            IsSwipeEnabled = false;

            Ideas.Clear();
            CurrentIdea = null;
            CurrentIdeaImage = ImageSource.FromFile("placeholder.png");

            try
            {
                // 1. Napi limit:
                string todayKey = $"LastVote_{type}_{DateTime.Now:yyyyMMdd}";
                if (Preferences.ContainsKey(todayKey))
                {
                    await Shell.Current.DisplayAlert("Mára ennyi!",
                        "Ebben a kategóriában mára elfogytak az ötletek. Gyere vissza holnap!",
                        "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // 2. Adatok:
                List<Idea> newItems = new();

                if (type == "Movie")
                {
                    newItems = await _movieService.GetPopularMoviesAsync();
                    if (newItems.Count == 0)
                        newItems = _ideaService.GetIdeas();
                }
                else if (type == "Food")
                {
                    newItems = _ideaService.GetFoodIdeas();
                }
                else if (type == "Date")
                {
                    newItems = _ideaService.GetDateIdeas();
                }

                if (ct.IsCancellationRequested) return;

                // 3. Randomizálás:
                var rnd = new Random();
                var shuffledItems = newItems.OrderBy(_ => rnd.Next()).ToList();

                foreach (var item in shuffledItems)
                    Ideas.Add(item);

                if (Ideas.Count > 0)
                {
                    _currentIndex = 0;
                    CurrentIdea = Ideas[0];
                    IsFinished = false;
                    IsSwipeEnabled = true;
                }
                else
                {
                    IsFinished = true;
                    IsSwipeEnabled = false;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hiba", ex.Message, "OK");
                IsFinished = true;
                IsSwipeEnabled = false;
            }
        }

        private ImageSource BuildImageSource(Idea idea)
        {
            var url = idea?.ImageUrlResolved;

            if (string.IsNullOrWhiteSpace(url))
                return ImageSource.FromFile("placeholder.png");

            // Web
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return new UriImageSource
                {
                    Uri = new Uri(url),
                    CachingEnabled = true,
                    CacheValidity = TimeSpan.FromDays(7)
                };
            }

            return ImageSource.FromFile(url);
        }

        [RelayCommand]
        private async Task SwipeRight()
        {
            if (!IsSwipeEnabled || IsFinished || CurrentIdea == null) return;

            IsSwipeEnabled = false;
            try
            {
                await _authService.VoteAsync(CurrentIdea.Title, true);
            }
            finally
            {
                NextCard();
            }
        }

        [RelayCommand]
        private async Task SwipeLeft()
        {
            if (!IsSwipeEnabled || IsFinished || CurrentIdea == null) return;

            IsSwipeEnabled = false;
            try
            {
                await _authService.VoteAsync(CurrentIdea.Title, false);
            }
            finally
            {
                NextCard();
            }
        }

        private void NextCard()
        {
            _currentIndex++;

            if (_currentIndex < Ideas.Count)
            {
                CurrentIdea = Ideas[_currentIndex];
                IsSwipeEnabled = true;
                return;
            }

            // Elfogyott
            CurrentIdea = null;
            CurrentIdeaImage = ImageSource.FromFile("placeholder.png");
            IsFinished = true;
            IsSwipeEnabled = false;

            string todayKey = $"LastVote_{CategoryType}_{DateTime.Now:yyyyMMdd}";
            Preferences.Set(todayKey, true);
        }

        [RelayCommand]
        private async Task BackToCategories()
        {
            await Shell.Current.GoToAsync("//CategoryPage");
        }
    }
}
