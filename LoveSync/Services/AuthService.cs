using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ModelUser = LoveSync.Models.User;
using FirebaseAuthUser = Firebase.Auth.User;
using VoteModel = LoveSync.Models.Vote;

namespace LoveSync.Services
{
    public class AuthService
    {
        private readonly FirebaseAuthClient _authClient;
        private readonly FirebaseClient _firebaseClient;

        public string AuthDomain = "lovesync-7a5ea.firebaseapp.com";

        private FirebaseAuthUser _currentAuthUser;

        public AuthService()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = Constants.FirebaseApiKey,
                AuthDomain = AuthDomain,
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },

                // Restart után is megmarad a bejelentkezés:
                UserRepository = new FileUserRepository("LoveSyncAuth")
            };

            _authClient = new FirebaseAuthClient(config);
            _firebaseClient = new FirebaseClient(Constants.FirebaseDatabaseUrl);

            // Session visszatöltése:
            _currentAuthUser = _authClient.User;
            if (!string.IsNullOrWhiteSpace(_currentAuthUser?.Uid))
                Preferences.Set("CurrentUserId", _currentAuthUser.Uid);
        }

        public string CurrentUserId
        {
            get
            {
                var uid = _authClient.User?.Uid;
                if (!string.IsNullOrWhiteSpace(uid))
                {
                    Preferences.Set("CurrentUserId", uid);
                    return uid;
                }
                return Preferences.Get("CurrentUserId", string.Empty);
            }
        }

        public bool IsSignedIn => !string.IsNullOrWhiteSpace(CurrentUserId);

        public async Task<string> LoginAsync(string email, string password)
        {
            var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(email, password);
            _currentAuthUser = userCredential.User;

            var userId = userCredential.User.Uid;
            Preferences.Set("CurrentUserId", userId);

            return userId;
        }

        public async Task RegisterAsync(string email, string password)
        {
            var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);
            _currentAuthUser = userCredential.User;

            var userId = userCredential.User.Uid;

            var user = new ModelUser
            {
                Id = userId,
                Email = email,
                PartnerId = null,
                PairingCode = null,
                IsPaired = false,
                ProfileImageBase64 = null
            };

            await _firebaseClient.Child("Users").Child(userId).PutAsync(user);
            Preferences.Set("CurrentUserId", userId);
        }

        public async Task<ModelUser> GetUserAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid)) return null;
            return await _firebaseClient.Child("Users").Child(uid).OnceSingleAsync<ModelUser>();
        }

        // ID token:
        public async Task<string> GetIdTokenAsync()
        {
            var u = _authClient.User;
            if (u == null) return null;

            return await u.GetIdTokenAsync();
        }

        public async Task<string> GeneratePairingCodeAsync()
        {
            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            string userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId))
                throw new Exception("Nincs bejelentkezve felhasználó!");

            await _firebaseClient.Child("Users").Child(userId).PatchAsync(new { PairingCode = code });
            return code;
        }

        public async Task<bool> PairWithUserAsync(string inputCode)
        {
            string myUserId = CurrentUserId;
            if (string.IsNullOrEmpty(myUserId))
                throw new Exception("Nincs bejelentkezve felhasználó!");

            var users = await _firebaseClient.Child("Users").OnceAsync<ModelUser>();
            var partner = users.FirstOrDefault(u => u.Object.PairingCode == inputCode);

            if (partner == null)
                throw new Exception("Hibás kód! Nem találtam ilyen felhasználót.");

            string partnerId = partner.Object.Id;

            await _firebaseClient.Child("Users").Child(myUserId)
                .PatchAsync(new { PartnerId = partnerId, IsPaired = true, PairingCode = (string)null });

            await _firebaseClient.Child("Users").Child(partnerId)
                .PatchAsync(new { PartnerId = myUserId, IsPaired = true, PairingCode = (string)null });

            return true;
        }

        public async Task UploadProfileImageAsync(string base64Image)
        {
            string userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return;

            await _firebaseClient
                .Child("Users")
                .Child(userId)
                .PatchAsync(new { ProfileImageBase64 = base64Image });
        }

        public async Task UnpairUsersAsync()
        {
            string myId = CurrentUserId;
            if (string.IsNullOrEmpty(myId)) return;

            var me = await GetUserAsync(myId);
            string partnerId = me?.PartnerId;

            await _firebaseClient
                .Child("Users")
                .Child(myId)
                .PatchAsync(new { PartnerId = (string)null, IsPaired = false });

            if (!string.IsNullOrEmpty(partnerId))
            {
                await _firebaseClient
                    .Child("Users")
                    .Child(partnerId)
                    .PatchAsync(new { PartnerId = (string)null, IsPaired = false });
            }
        }

        // 1. Ezt hívja a SwipeViewModel
        // Automatikusan hozzáadjuk a CurrentUserId-t
        public Task VoteAsync(string ideaTitle, string category, bool isLiked)
            => VoteAsync(CurrentUserId, ideaTitle, category, isLiked);

        // 2. Mentést végző metódus (bővítve a category paraméterrel)
        public async Task VoteAsync(string userId, string ideaTitle, string category, bool isLiked)
        {
            if (string.IsNullOrWhiteSpace(userId))
                userId = CurrentUserId;

            if (string.IsNullOrWhiteSpace(userId)) return;

            var vote = new VoteModel
            {
                UserId = userId,
                IdeaId = ideaTitle,
                Category = category,
                IsLiked = isLiked,
                Timestamp = DateTime.UtcNow
            };

            await _firebaseClient.Child("Votes").Child(userId).PostAsync(vote);
        }

        public Task VoteAsync(string ideaTitle, bool isLiked)
            => VoteAsync(CurrentUserId, ideaTitle, null, isLiked);


        public async Task<List<VoteModel>> GetVotesForUserAsync(string userId)
        {
            try
            {
                var votes = await _firebaseClient.Child("Votes").Child(userId).OnceAsync<VoteModel>();
                return votes.Select(v => v.Object).Where(v => v != null).ToList();
            }
            catch
            {
                return new List<VoteModel>();
            }
        }

        public void SignOut()
        {
            _authClient.SignOut();
            Preferences.Remove("CurrentUserId");
            _currentAuthUser = null;
        }
    }
}