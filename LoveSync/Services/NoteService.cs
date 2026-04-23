using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using LoveSync.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace LoveSync.Services
{
    public class NoteService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly AuthService _authService;

        public NoteService() : this(new AuthService())
        {
        }

        public NoteService(AuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            _firebaseClient = new FirebaseClient(
                Constants.FirebaseDatabaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = async () =>
                    {
                        var token = await _authService.GetIdTokenAsync();
                        return token ?? string.Empty;
                    }
                });
        }

        private string RequireUserId()
        {
            var uid = _authService.CurrentUserId;
            if (string.IsNullOrWhiteSpace(uid))
                throw new InvalidOperationException("Nincs bejelentkezett felhasználó (CurrentUserId üres).");
            return uid;
        }

        public async Task<string> AddNoteAsync(string text)
        {
            string userId = RequireUserId();

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Az üzenet nem lehet üres.", nameof(text));

            var user = await _authService.GetUserAsync(userId);

            var note = new Note
            {
                UserId = userId,
                UserName = user?.Email ?? "Ismeretlen",
                UserImageBase64 = null,
                Text = text,
                Timestamp = DateTime.UtcNow
            };

            var created = await _firebaseClient
                .Child("Notes")
                .Child(userId)
                .PostAsync(note);

            return created?.Key;
        }

        public async Task<List<Note>> GetNotesAsync()
        {
            string myId = Preferences.Get("CurrentUserId", string.Empty);
            if (string.IsNullOrEmpty(myId)) return new List<Note>();

            var allNotes = new List<Note>();

            try
            {
                // 1. Saját üzenetek
                var myNotes = await _firebaseClient
                    .Child("Notes")
                    .Child(myId)
                    .OnceAsync<Note>();

                foreach (var n in myNotes)
                {
                    if (n?.Object == null) continue;
                    n.Object.Id = $"{myId}:{n.Key}";
                    allNotes.Add(n.Object);
                }

                // 2. Partner üzenetek
                var me = await _authService.GetUserAsync(myId);
                string partnerId = me?.PartnerId;

                if (!string.IsNullOrWhiteSpace(partnerId))
                {
                    var partnerNotes = await _firebaseClient
                        .Child("Notes")
                        .Child(partnerId)
                        .OnceAsync<Note>();

                    foreach (var n in partnerNotes)
                    {
                        if (n?.Object == null) continue;
                        n.Object.Id = $"{partnerId}:{n.Key}";
                        allNotes.Add(n.Object);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetNotesAsync error: {ex.Message}");
            }

            return allNotes.OrderBy(x => x.Timestamp).ToList();
        }

        public IObservable<FirebaseEvent<Note>> ListenToNotes(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId nem lehet üres.", nameof(userId));

            return _firebaseClient
                .Child("Notes")
                .Child(userId)
                .AsObservable<Note>()
                .Where(e => !string.IsNullOrWhiteSpace(e.Key));
        }
    }
}