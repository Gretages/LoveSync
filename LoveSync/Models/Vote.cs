namespace LoveSync.Models
{
    public class Vote
    {
        public string UserId { get; set; }
        public string IdeaId { get; set; }
        public bool IsLiked { get; set; }
        public string Category { get; set; }
        public DateTime Timestamp { get; set; }
    }
}