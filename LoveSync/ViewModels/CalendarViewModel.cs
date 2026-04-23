using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database.Streaming;
using LoveSync.Models;
using LoveSync.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LoveSync.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private readonly CalendarService _calendarService = new CalendarService();
        private readonly AuthService _authService = new AuthService();

        public ObservableCollection<CalendarEvent> Events { get; set; } = new();

        private IDisposable _mySub;
        private IDisposable _partnerSub;

        private string _myId;
        private string _partnerId;

        private bool _isInitialized = false;

        [ObservableProperty]
        string newEventTitle;

        [ObservableProperty]
        string newEventLocation;

        [ObservableProperty]
        DateTime newEventDate = DateTime.Now;

        [ObservableProperty]
        TimeSpan newEventTime = TimeSpan.FromHours(18);

        [ObservableProperty]
        bool isBusy;

        public CalendarViewModel()
        {
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            await LoadEvents();
        }

        public async Task LoadEvents()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // 1. Azonosítók betöltése
                _myId = _authService.CurrentUserId;
                if (!string.IsNullOrEmpty(_myId))
                {
                    var me = await _authService.GetUserAsync(_myId);
                    _partnerId = me?.PartnerId;
                }

                // 2. Előzmények betöltése 
                var list = await _calendarService.GetEventsAsync();

                // UI frissítése
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Events.Clear();
                    foreach (var evt in list)
                    {
                        Events.Add(evt);
                    }
                });

                // 3. Figyelés indítása
                StartListening();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Calendar Load Error: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void StartListening()
        {
            _mySub?.Dispose();
            _partnerSub?.Dispose();

            if (!string.IsNullOrEmpty(_myId))
            {
                _mySub = _calendarService.ListenToCalendar(_myId)
                    .Subscribe(e => ApplyRealtimeEvent(_myId, e));
            }

            if (!string.IsNullOrEmpty(_partnerId))
            {
                _partnerSub = _calendarService.ListenToCalendar(_partnerId)
                    .Subscribe(e => ApplyRealtimeEvent(_partnerId, e));
            }
        }

        private void ApplyRealtimeEvent(string ownerId, FirebaseEvent<CalendarEvent> e)
        {
            if (e.Key == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                string uniqueId = $"{ownerId}_{e.Key}";

                // TÖRLÉS
                if (e.EventType == FirebaseEventType.Delete)
                {
                    // Megpróbáljuk megtalálni a listában
                    var toRemove = Events.FirstOrDefault(x => x.Id == e.Key);
                    if (toRemove == null) toRemove = Events.FirstOrDefault(x => x.Id == uniqueId);

                    if (toRemove != null)
                    {
                        Events.Remove(toRemove);
                        // Értesítés visszavonása, ha törölték az eseményt
                        LocalNotificationCenter.Current.Cancel(GetNotificationId(e.Key));
                    }
                    return;
                }

                // MÓDOSÍTÁS / HOZZÁADÁS
                var evt = e.Object;
                evt.Id = e.Key;
                evt.UserId = ownerId;

                var existing = Events.FirstOrDefault(x => x.Id == evt.Id);

                if (existing != null)
                {
                    // Update
                    existing.Title = evt.Title;
                    existing.Location = evt.Location;
                    existing.Date = evt.Date;
                }
                else
                {
                    // Insert
                    Events.Add(evt);
                    // Dátum szerinti rendezés fenntartása
                    var sorted = Events.OrderBy(x => x.Date).ToList();
                    Events.Clear();
                    foreach (var item in sorted) Events.Add(item);
                }

                // Értesítés (mindenkinek, automatikusan)
                if (evt.Date > DateTime.Now)
                {
                    ScheduleNotification(evt);
                }
            });
        }

        private void ScheduleNotification(CalendarEvent evt)
        {
#if ANDROID || IOS
            var notifyTime = evt.Date.AddHours(-1);
            // Ha kevesebb mint 1 óra van hátra, de még a jövőben van, akkor 10 mp múlva jelezzen
            if (notifyTime < DateTime.Now) notifyTime = DateTime.Now.AddSeconds(10);

            var request = new NotificationRequest
            {
                NotificationId = GetNotificationId(evt.Id),
                Title = "Randi emlékeztető! ❤️",
                Description = $"{evt.Title} @ {evt.Location} ({evt.Date:HH:mm})",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = notifyTime
                }
            };
            LocalNotificationCenter.Current.Show(request);
#endif
        }

        private int GetNotificationId(string firebaseKey)
        {
            return Math.Abs(firebaseKey.GetHashCode());
        }

        [RelayCommand]
        async Task AddEvent()
        {
            if (string.IsNullOrWhiteSpace(NewEventTitle)) return;

            DateTime fullDateTime = NewEventDate.Date + NewEventTime;

            // Ab-ba küldés:
            await _calendarService.AddEventAsync(NewEventTitle, NewEventLocation, fullDateTime);

            NewEventTitle = string.Empty;
            NewEventLocation = string.Empty;
        }

        [RelayCommand]
        async Task DeleteEvent(CalendarEvent evt)
        {
            if (evt == null) return;
            bool confirm = await Shell.Current.DisplayAlert("Törlés", "Biztos törlöd?", "Igen", "Nem");
            if (confirm)
            {
                await _calendarService.DeleteEventAsync(evt);
            }
        }

        [RelayCommand]
        async Task OpenMap(CalendarEvent evt)
        {
            if (evt == null || string.IsNullOrWhiteSpace(evt.Location)) return;
            try
            {
                await Launcher.OpenAsync($"geo:0,0?q={Uri.EscapeDataString(evt.Location)}");
            }
            catch { }
        }
    }
}