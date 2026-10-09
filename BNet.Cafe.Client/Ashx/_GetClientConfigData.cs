using Newtonsoft.Json;

namespace BNet.Cafe.Client.Ashx
{
    public class GetClientConfigData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("accountCreationAllowed")]
        public bool AccountCreationAllowed { get; set; }

        [JsonProperty("autoShutDownInterval")]
        public int AutoShutDownInterval { get; set; }

        [JsonProperty("desktopSlideShow")]
        public bool DesktopSlideShow { get; set; }

        [JsonProperty("resetShutdownCountdown")]
        public bool ResetShutdownCountdown { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("timeCreated")]
        public string TimeCreated { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
    }
}