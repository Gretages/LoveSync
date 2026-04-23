using Newtonsoft.Json;
using System.Collections.Generic;

namespace LoveSync.Models
{
    // A válasz főszerkezete, amit az API visszaküld:
    public class TmdbResponse
    {
        [JsonProperty("results")]
        public List<TmdbMovie> Results { get; set; }
    }

    // Egy film adatai:
    public class TmdbMovie
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }

        [JsonProperty("vote_average")]
        public double VoteAverage { get; set; }

        // Összeállítja a teljes kép URL-t:
        public string FullPosterUrl => string.IsNullOrEmpty(PosterPath)
            ? "https://via.placeholder.com/500x750?text=No+Image"
            : $"https://image.tmdb.org/t/p/w500{PosterPath}";
    }
}