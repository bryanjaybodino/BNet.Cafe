using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    public class UserData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("timeCreated")]
        public string TimeCreated { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("totalDuration")]
        public int TotalDuration { get; set; }

        [JsonProperty("formattedTotalDuration")]
        public string FormattedTotalDuration { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }   
    }
}
