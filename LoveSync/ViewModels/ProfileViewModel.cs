using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LoveSync.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System;
using System.IO;

namespace LoveSync.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly AuthService _authService = new AuthService();
        private readonly IdeaService _ideaService = new IdeaService();

        [ObservableProperty]
        string email;

        [ObservableProperty]
        string partnerEmail;

        [ObservableProperty]
        ImageSource profileImage;

        [ObservableProperty]
        int totalVotes;

        [ObservableProperty]
        int matchCount;

        [ObservableProperty]
        string favoriteCategory;

        [ObservableProperty]
        bool isBusy;

        public ProfileViewModel()
        {
            LoadProfileDataCommand.Execute(null);
        }

        [RelayCommand]
        async Task LoadProfileData()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                string myId = Preferences.Get("CurrentUserId", string.Empty);
                if (string.IsNullOrEmpty(myId)) return;

                // 1. Felhasználói adatok
                var me = await _authService.GetUserAsync(myId);
                Email = me.Email;

                // Kép betöltése
                if (!string.IsNullOrEmpty(me.ProfileImageBase64))
                {
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(me.ProfileImageBase64);
                        ProfileImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }
                    catch { ProfileImage = null; }
                }
                else
                {
                    ProfileImage = null;
                }

                string partnerId = me.PartnerId;
                if (!string.IsNullOrEmpty(partnerId))
                {
                    var partner = await _authService.GetUserAsync(partnerId);
                    PartnerEmail = partner?.Email ?? "Nincs adat";
                }
                else
                {
                    PartnerEmail = "Nincs pár";
                }

                // 2. Szavazatok:
                var myVotes = await _authService.GetVotesForUserAsync(myId);
                TotalVotes = myVotes.Count;

                // 3. Matchek:
                if (!string.IsNullOrEmpty(partnerId))
                {
                    var partnerVotes = await _authService.GetVotesForUserAsync(partnerId);

                    var myLikes = myVotes.Where(v => v.IsLiked).Select(v => v.IdeaId).ToList();
                    var partnerLikes = partnerVotes.Where(v => v.IsLiked).Select(v => v.IdeaId).ToList();

                    var matches = myLikes.Intersect(partnerLikes).ToList();
                    MatchCount = matches.Count;

                    if (matches.Count > 0)
                    {
                        var categoryCounts = new Dictionary<string, int>();

                        foreach (var title in matches)
                        {
                            var idea = _ideaService.GetIdeaByTitle(title);
                            if (idea != null)
                            {
                                if (!categoryCounts.ContainsKey(idea.Category))
                                    categoryCounts[idea.Category] = 0;

                                categoryCounts[idea.Category]++;
                            }
                        }

                        if (categoryCounts.Count > 0)
                        {
                            var top = categoryCounts.OrderByDescending(x => x.Value).First();
                            FavoriteCategory = TranslateCategory(top.Key);
                        }
                        else
                        {
                            FavoriteCategory = "-";
                        }
                    }
                    else
                    {
                        FavoriteCategory = "-";
                    }
                }
                else
                {
                    // Ha nincs pár:
                    MatchCount = 0;
                    FavoriteCategory = "-";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string TranslateCategory(string cat)
        {
            return cat switch
            {
                "Movie" => "Filmek 🎬",
                "Food" => "Kaja 🍔",
                "Date" => "Randi 🌳",
                _ => cat
            };
        }

        [RelayCommand]
        async Task ChangeProfilePicture()
        {
            string action = await Shell.Current.DisplayActionSheet("Profilkép módosítása", "Mégse", null, "Fotó készítése 📸", "Választás a galériából 🖼️");

            if (action == "Mégse" || action == null) return;

            FileResult photo = null;

            try
            {
                if (action == "Fotó készítése 📸")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                        photo = await MediaPicker.Default.CapturePhotoAsync();
                    else
                        await Shell.Current.DisplayAlert("Hiba", "A kamera nem támogatott.", "OK");
                }
                else if (action == "Választás a galériából 🖼️")
                {
                    photo = await MediaPicker.Default.PickPhotoAsync();
                }

                if (photo != null)
                {
                    using var stream = await photo.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();

                    ProfileImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));

                    string base64String = Convert.ToBase64String(imageBytes);
                    await _authService.UploadProfileImageAsync(base64String);

                    await Shell.Current.DisplayAlert("Siker", "Profilkép frissítve!", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Hiba", $"Nem sikerült a képkezelés: {ex.Message}", "OK");
            }
        }

        // Szakítás:
        [RelayCommand]
        async Task Unpair()
        {
            if (string.IsNullOrEmpty(PartnerEmail) || PartnerEmail == "Nincs pár")
            {
                await Shell.Current.DisplayAlert("Hiba", "Jelenleg nem vagy párkapcsolatban.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Szakítás 💔",
                "Biztosan meg akarod szüntetni a kapcsolatot a pároddal? Mindkét fiók újra párosítható lesz.",
                "Igen, szakítok", "Mégse");

            if (confirm)
            {
                IsBusy = true;
                try
                {
                    await _authService.UnpairUsersAsync();
                    await Shell.Current.DisplayAlert("Kész", "A kapcsolat megszűnt.", "OK");

                    // Adatok frissítése:
                    await LoadProfileData();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Hiba", $"Hiba történt: {ex.Message}", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        [RelayCommand]
        async Task Logout()
        {
            bool answer = await Shell.Current.DisplayAlert("Kijelentkezés", "Biztosan kilépsz?", "Igen", "Nem");
            if (answer)
            {
                Preferences.Remove("CurrentUserId");
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}