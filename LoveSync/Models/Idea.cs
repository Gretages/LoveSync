using System;

namespace LoveSync.Models
{
    public class Idea
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }

        // Minden betöltéskor/klónozáskor -> frissülnek
        public long ImageVersion { get; set; } = DateTime.UtcNow.Ticks;

        // Biztonságos URL/fallback:
        public string ImageUrlResolved =>
            string.IsNullOrWhiteSpace(ImageUrl) ? "placeholder.png" : ImageUrl;

        // Webes kép:
        public string ImageUrlCacheBusted
        {
            get
            {
                var url = ImageUrlResolved;

                if (string.IsNullOrWhiteSpace(url) ||
                    !url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    return url;

                var sep = url.Contains("?") ? "&" : "?";
                return $"{url}{sep}v={ImageVersion}";
            }
        }
    }
}