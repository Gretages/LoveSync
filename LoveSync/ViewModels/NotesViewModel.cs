using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Database.Streaming;
using LoveSync.Models;
using LoveSync.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Plugin.LocalNotification; // Ez kell az értesítéshez
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LoveSync.ViewModels
{
    public partial class NotesViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly NoteService _noteService;

        private IDisposable _mySub;
        private IDisposable _partnerSub;

        private string _myId;
        private string _partnerId;

        private string _myProfileImageBase64;
        private string _partnerProfileImageBase64;
        private string _myName;

        private bool _isInitialized = false;

        public ObservableCollection<Note> Notes { get; } = new();

        [ObservableProperty]
        private string newNoteText;

        [ObservableProperty]
        private bool isBusy;

        public NotesViewModel()
        {
            _authService = new AuthService();
            _noteService = new NoteService(_authService);
        }



        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            IsBusy = true;
            try
            {
                await LoadProfileContext();
                await LoadHistoryAndMerge();
                StartListening();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Notes init hiba: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // A régi OnAppearing parancsot EGYSZERŰSÍTSD LE erre:
        [RelayCommand]
        private async Task OnAppearing()
        {
            // Már nem töltünk be mindent újra, csak ha még sosem futott
            if (!_isInitialized)
            {
                await InitializeAsync();
            }
        }

        [RelayCommand]
        private Task OnDisappearing()
        {
            StopListening();
            return Task.CompletedTask;
        }

        private async Task LoadProfileContext()
        {
            _myId = _authService.CurrentUserId;
            if (string.IsNullOrWhiteSpace(_myId)) return;

            var me = await _authService.GetUserAsync(_myId);
            if (me == null) return;

            _myName = me.Email;
            _partnerId = me.PartnerId;
            _myProfileImageBase64 = me.ProfileImageBase64;

            _partnerProfileImageBase64 = null;
            if (!string.IsNullOrWhiteSpace(_partnerId))
            {
                var partner = await _authService.GetUserAsync(_partnerId);
                _partnerProfileImageBase64 = partner?.ProfileImageBase64;
            }
        }

        private async Task LoadHistoryAndMerge()
        {
            var list = await _noteService.GetNotesAsync();

            foreach (var n in list) AttachProfileImage(n);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Notes.Clear();
                foreach (var n in list.OrderBy(x => x.Timestamp))
                {
                    Notes.Add(n);
                }
            });
        }

        private void StartListening()
        {
            StopListening();

            if (!string.IsNullOrEmpty(_myId))
            {
                _mySub = _noteService.ListenToNotes(_myId)
                    .Subscribe(e => ApplyRealtimeEvent(_myId, e));
            }

            if (!string.IsNullOrWhiteSpace(_partnerId))
            {
                _partnerSub = _noteService.ListenToNotes(_partnerId)
                    .Subscribe(e => ApplyRealtimeEvent(_partnerId, e));
            }
        }

        private void StopListening()
        {
            _mySub?.Dispose();
            _partnerSub?.Dispose();
        }

        private void ApplyRealtimeEvent(string ownerUserId, FirebaseEvent<Note> e)
        {
            if (e.Object == null && e.EventType != FirebaseEventType.Delete) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                string key = e.Key;
                string uniqueId = $"{ownerUserId}:{key}";

                if (e.EventType == FirebaseEventType.Delete)
                {
                    var toRemove = Notes.FirstOrDefault(n => n.Id == uniqueId);
                    if (toRemove != null) Notes.Remove(toRemove);
                    return;
                }

                var note = e.Object;
                note.Id = uniqueId;
                if (string.IsNullOrEmpty(note.UserId)) note.UserId = ownerUserId;

                AttachProfileImage(note);

                var existing = Notes.FirstOrDefault(n => n.Id == uniqueId);
                if (existing != null)
                {
                    existing.Text = note.Text;
                    existing.Timestamp = note.Timestamp;
                }
                else
                {
                    InsertNoteOrdered(note);

                    // Értesítés csak akkor, ha a partner küldte az üzenetet, és az most érkezett:
                    if (note.UserId == _partnerId && (DateTime.UtcNow - note.Timestamp).TotalMinutes < 1)
                    {
                        ShowNotification(note.Text);
                    }
                }
            });
        }

        private void ShowNotification(string message)
        {
            var request = new NotificationRequest
            {
                NotificationId = new Random().Next(1000, 9999),
                Title = "Új üzenet! ❤️",
                Description = message,
                BadgeNumber = 1,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Now.AddSeconds(1)
                }
            };

            LocalNotificationCenter.Current.Show(request);
        }

        private void InsertNoteOrdered(Note note)
        {
            int i = 0;
            while (i < Notes.Count && Notes[i].Timestamp <= note.Timestamp)
            {
                if (Notes[i].Id == note.Id) return;
                i++;
            }
            Notes.Insert(i, note);
        }

        private void AttachProfileImage(Note note)
        {
            if (note == null) return;
            if (note.UserId == _myId) note.UserImageBase64 = _myProfileImageBase64;
            else if (note.UserId == _partnerId) note.UserImageBase64 = _partnerProfileImageBase64;
        }

        [RelayCommand]
        private async Task SendNote()
        {
            if (string.IsNullOrWhiteSpace(NewNoteText)) return;

            string text = NewNoteText;
            NewNoteText = string.Empty;

            var tempNote = new Note
            {
                Id = Guid.NewGuid().ToString(),
                UserId = _myId,
                UserName = string.IsNullOrWhiteSpace(_myName) ? "Én" : _myName,
                Text = text,
                Timestamp = DateTime.UtcNow,
                UserImageBase64 = _myProfileImageBase64
            };

            InsertNoteOrdered(tempNote);

            try
            {
                string key = await _noteService.AddNoteAsync(text);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    string realId = $"{_myId}:{key}";
                    var alreadyArrived = Notes.FirstOrDefault(n => n.Id == realId);
                    if (alreadyArrived != null) Notes.Remove(tempNote);
                    else tempNote.Id = realId;
                }
            }
            catch (Exception ex)
            {
                Notes.Remove(tempNote);
                NewNoteText = text;
                await Shell.Current.DisplayAlert("Hiba", "Nem sikerült elküldeni: " + ex.Message, "OK");
            }
        }
    }
}