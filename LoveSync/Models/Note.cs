using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.IO;

namespace LoveSync.Models
{
    public class Note
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        public string UserImageBase64 { get; set; }

        [JsonIgnore]
        public ImageSource ProfileImageSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserImageBase64))
                    return ImageSource.FromFile("user_icon.png");

                // Felhasználó profilkép:
                string raw = UserImageBase64;
                int commaIdx = raw.IndexOf(',');
                if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIdx >= 0)
                    raw = raw[(commaIdx + 1)..];

                try
                {
                    byte[] imageBytes = Convert.FromBase64String(raw);
                    return ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
                catch
                {
                    return ImageSource.FromFile("user_icon.png");
                }
            }
        }
    }
}
