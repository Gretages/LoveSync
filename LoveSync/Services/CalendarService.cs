using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using LoveSync.Models;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LoveSync.Services
{
    public class CalendarService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly AuthService _authService;

        public CalendarService() : this(new AuthService())
        {
        }

        public CalendarService(AuthService authService)
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

        public async Task AddEventAsync(string title, string location, DateTime date)
        {
            string myId = Preferences.Get("CurrentUserId", string.Empty);
            if (string.IsNullOrEmpty(myId)) return;

            var newEvent = new CalendarEvent
            {
                Title = title,
                Location = location,
                Date = date,
                UserId = myId
            };

            await _firebaseClient
                .Child("Calendar")
                .Child(myId)
                .PostAsync(newEvent);
        }

        public async Task DeleteEventAsync(CalendarEvent evt)
        {
            await _firebaseClient
                .Child("Calendar")
                .Child(evt.UserId)
                .Child(evt.Id)
                .DeleteAsync();
        }

        public async Task<List<CalendarEvent>> GetEventsAsync()
        {
            string myId = Preferences.Get("CurrentUserId", string.Empty);

            // Ellenőrizzük az ID-t:
            if (string.IsNullOrEmpty(myId))
            {
                System.Diagnostics.Debug.WriteLine("[Calendar] HIBA: Nincs UserId!");
                return new List<CalendarEvent>();
            }

            var me = await _authService.GetUserAsync(myId);
            string partnerId = me?.PartnerId;

            var allEvents = new List<CalendarEvent>();

            try
            {
                // 1. Saját:
                var myEvents = await GetEventsForUser(myId);
                allEvents.AddRange(myEvents);
                System.Diagnostics.Debug.WriteLine($"[Calendar] Saját események: {myEvents.Count} db");

                // 2. Partner:
                if (!string.IsNullOrEmpty(partnerId))
                {
                    var partnerEvents = await GetEventsForUser(partnerId);
                    allEvents.AddRange(partnerEvents);
                    System.Diagnostics.Debug.WriteLine($"[Calendar] Partner események: {partnerEvents.Count} db");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Calendar] Hiba a betöltéskor: {ex.Message}");
            }

            return allEvents.OrderBy(e => e.Date).ToList();
        }

        private async Task<List<CalendarEvent>> GetEventsForUser(string userId)
        {
            try
            {
                var events = await _firebaseClient
                    .Child("Calendar")
                    .Child(userId)
                    .OnceAsync<CalendarEvent>();

                return events.Select(x =>
                {
                    var evt = x.Object;
                    evt.Id = x.Key;
                    return evt;
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Calendar] Hiba a User({userId}) lekérésekor: {ex.Message}");
                return new List<CalendarEvent>();
            }
        }

        public IObservable<FirebaseEvent<CalendarEvent>> ListenToCalendar(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Observable.Empty<FirebaseEvent<CalendarEvent>>();

            return _firebaseClient
                .Child("Calendar")
                .Child(userId)
                .AsObservable<CalendarEvent>()
                .Where(e => !string.IsNullOrWhiteSpace(e.Key));
        }
    }
}