using Newtonsoft.Json;

namespace LoveSync.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PartnerId { get; set; }
        public string PairingCode { get; set; }
        public bool IsPaired { get; set; }

        [JsonProperty("ProfileImageBase64")]
        public string ProfileImageBase64 { get; set; }

        [JsonProperty("profileImageBase64")]
        public string ProfileImageBase64_Legacy { get; set; }

        [JsonIgnore]
        public string ProfileImageBase64Resolved =>
            !string.IsNullOrWhiteSpace(ProfileImageBase64)
                ? ProfileImageBase64
                : ProfileImageBase64_Legacy;
    }
}
