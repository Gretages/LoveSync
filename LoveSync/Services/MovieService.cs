using LoveSync.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace LoveSync.Services
{
    public class MovieService
    {
        private const string ApiKey = "c3e1552fbd2e6d02932089950a6eecfa";
        private const string BaseUrl = "https://api.themoviedb.org/3";
        private readonly HttpClient _httpClient;

        public MovieService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
        }

        public async Task<List<Idea>> GetPopularMoviesAsync()
        {
            string url = $"{BaseUrl}/movie/popular?api_key={ApiKey}&language=hu-HU&page=1";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var tmdbData = JsonConvert.DeserializeObject<TmdbResponse>(response);

                var ideas = (tmdbData?.Results ?? new List<TmdbMovie>())
                    .Select(m => new Idea
                    {
                        Id = m.Id.ToString(),
                        Title = m.Title,
                        Description = m.Overview,
                        ImageUrl = m.FullPosterUrl,
                        Category = "Movie"
                    })
                    .ToList();

                return ideas;
            }
            catch
            {
                return new List<Idea>();
            }
        }

        // Cím alapján keresünk:
        public async Task<Idea> SearchMovieByTitleAsync(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            // HU + EN fallback:
            string huUrl = $"{BaseUrl}/search/movie?api_key={ApiKey}&language=hu-HU&include_adult=false&query={Uri.EscapeDataString(title)}";
            string enUrl = $"{BaseUrl}/search/movie?api_key={ApiKey}&language=en-US&include_adult=false&query={Uri.EscapeDataString(title)}";

            try
            {
                var huTask = _httpClient.GetStringAsync(huUrl);
                var enTask = _httpClient.GetStringAsync(enUrl);
                await Task.WhenAll(huTask, enTask);

                var hu = JsonConvert.DeserializeObject<TmdbResponse>(huTask.Result);
                var en = JsonConvert.DeserializeObject<TmdbResponse>(enTask.Result);

                var huFirst = (hu?.Results ?? new List<TmdbMovie>()).FirstOrDefault();
                var enFirst = (en?.Results ?? new List<TmdbMovie>()).FirstOrDefault();

                var best = huFirst ?? enFirst;
                if (best == null)
                    return null;

                var overview = !string.IsNullOrWhiteSpace(best.Overview)
                    ? best.Overview
                    : (enFirst?.Overview ?? "Közös találat");

                var poster = string.IsNullOrWhiteSpace(best.FullPosterUrl)
                    ? "placeholder.png"
                    : best.FullPosterUrl;

                return new Idea
                {
                    Id = best.Id.ToString(),
                    Title = best.Title ?? title,
                    Description = overview,
                    ImageUrl = poster,
                    Category = "Movie"
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
