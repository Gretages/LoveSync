using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoveSync.Models;
using LoveSync.Services;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LoveSync.ViewModels
{
    public partial class MatchesViewModel : ObservableObject
    {
        private readonly AuthService _authService = new AuthService();
        private readonly IdeaService _ideaService = new IdeaService();
        private readonly MovieService _movieService = new MovieService();

        [ObservableProperty]
        private ObservableCollection<Idea> matches = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage;

        public MatchesViewModel()
        {
            StatusMessage = "Keresés...";
        }

        private bool _isInitialized = false;

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            await LoadMatches();
        }

        [RelayCommand]
        public async Task OnAppearing()
        {
            await LoadMatches();
        }

        [RelayCommand]
        public async Task LoadMatches()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = "Keresés...";

            try
            {
                string myId = Preferences.Get("CurrentUserId", string.Empty);
                if (string.IsNullOrEmpty(myId))
                {
                    StatusMessage = "Nincs bejelentkezett felhasználó.";
                    Matches = new ObservableCollection<Idea>();
                    return;
                }

                var me = await _authService.GetUserAsync(myId);
                string partnerId = me?.PartnerId;

                if (string.IsNullOrEmpty(partnerId))
                {
                    StatusMessage = "Nincs párod, így közös találat sincs.";
                    Matches = new ObservableCollection<Idea>();
                    return;
                }

                var myVotes = await _authService.GetVotesForUserAsync(myId);
                var partnerVotes = await _authService.GetVotesForUserAsync(partnerId);

                var myUniqueLikes = myVotes
                    .Where(v => v.IsLiked)
                    .GroupBy(v => v.IdeaId)
                    .Select(g => g.First())
                    .ToList();

                var partnerLikedIds = partnerVotes
                    .Where(v => v.IsLiked)
                    .Select(v => v.IdeaId)
                    .ToHashSet();

                var commonMatches = myUniqueLikes
                    .Where(v => partnerLikedIds.Contains(v.IdeaId))
                    .ToList();

                if (commonMatches.Count == 0)
                {
                    StatusMessage = "Még nincs közös találat. Szavazzatok többet!";
                    Matches = new ObservableCollection<Idea>();
                    return;
                }

                StatusMessage = $"Találat: {commonMatches.Count} db";

                var nowVersion = DateTime.UtcNow.Ticks;
                var finalList = new List<Idea>();

                foreach (var vote in commonMatches)
                {
                    string key = vote.IdeaId;

                    string savedCategory = vote.Category;
                    string fallbackCategory = !string.IsNullOrEmpty(savedCategory) ? savedCategory : "Movie";

                    // 1) Próbáljuk meg a helyi listából (Food/Date)
                    var ideaFull = _ideaService.GetIdeaByTitle(key);

                    // 2) Ha nincs helyi adat, és Movie, akkor próbáljuk meg TMDB-ből
                    if (ideaFull == null && fallbackCategory == "Movie")
                    {
                        var apiIdea = await _movieService.SearchMovieByTitleAsync(key);
                        if (apiIdea != null)
                            ideaFull = apiIdea;
                    }

                    // 3) Ha még mindig nincs, placeholder
                    if (ideaFull == null)
                    {
                        ideaFull = new Idea
                        {
                            Id = Guid.NewGuid().ToString(),
                            Title = key,
                            Description = "Közös találat",
                            Category = fallbackCategory,
                            ImageUrl = "placeholder.png"
                        };
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(ideaFull.ImageUrl))
                            ideaFull.ImageUrl = "placeholder.png";

                        if (!string.IsNullOrEmpty(savedCategory))
                            ideaFull.Category = savedCategory;
                    }

                    finalList.Add(new Idea
                    {
                        Id = ideaFull.Id,
                        Title = ideaFull.Title,
                        Description = ideaFull.Description,
                        Category = ideaFull.Category,
                        ImageUrl = ideaFull.ImageUrl,
                        ImageVersion = nowVersion
                    });
                }

                Matches = new ObservableCollection<Idea>(finalList);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Hiba: {ex.Message}";
                Matches = new ObservableCollection<Idea>();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
