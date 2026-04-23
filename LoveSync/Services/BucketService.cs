using Firebase.Database;
using Firebase.Database.Query;
using LoveSync.Models;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoveSync.Services
{
    public class BucketService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly AuthService _authService;

        public BucketService()
        {
            _firebaseClient = new FirebaseClient(Constants.FirebaseDatabaseUrl);
            _authService = new AuthService();
        }

        // Új elem hozzáadása:
        public async Task AddItemAsync(string title)
        {
            string myId = Preferences.Get("CurrentUserId", string.Empty);
            if (string.IsNullOrEmpty(myId)) return;

            var item = new BucketItem
            {
                Title = title,
                IsCompleted = false,
                UserId = myId 
            };

            await _firebaseClient
                .Child("BucketList")
                .Child(myId)
                .PostAsync(item);
        }

        // Elem módosítása:
        public async Task UpdateItemAsync(BucketItem item)
        {
            // Annak a mappájában keressük, aki létrehozta (item.UserId):
            await _firebaseClient
                .Child("BucketList")
                .Child(item.UserId)
                .Child(item.Id)
                .PutAsync(item);
        }

        // Törlés
        public async Task DeleteItemAsync(BucketItem item)
        {
            await _firebaseClient
                .Child("BucketList")
                .Child(item.UserId)
                .Child(item.Id)
                .DeleteAsync();
        }

        // Összes elem lekérése (Saját + Partner)
        public async Task<List<BucketItem>> GetBucketListAsync()
        {
            string myId = Preferences.Get("CurrentUserId", string.Empty);
            if (string.IsNullOrEmpty(myId)) return new List<BucketItem>();

            var me = await _authService.GetUserAsync(myId);
            string partnerId = me?.PartnerId;

            // 1. Saját:
            var myItems = await GetItemsForUser(myId);

            // 2. Partner:
            var partnerItems = new List<BucketItem>();
            if (!string.IsNullOrEmpty(partnerId))
            {
                partnerItems = await GetItemsForUser(partnerId);
            }

            // 3. Összefésülés:
            var allItems = new List<BucketItem>();
            allItems.AddRange(myItems);
            allItems.AddRange(partnerItems);

            return allItems;
        }

        private async Task<List<BucketItem>> GetItemsForUser(string userId)
        {
            try
            {
                var items = await _firebaseClient
                    .Child("BucketList")
                    .Child(userId)
                    .OnceAsync<BucketItem>();

                return items.Select(x =>
                {
                    var item = x.Object;
                    item.Id = x.Key; 
                    return item;
                }).ToList();
            }
            catch
            {
                return new List<BucketItem>();
            }
        }
    }
}