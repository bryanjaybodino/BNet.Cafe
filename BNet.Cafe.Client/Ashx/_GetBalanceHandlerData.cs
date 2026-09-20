using Newtonsoft.Json;

namespace BNet.Cafe.Client.Ashx
{
    public class GetBalanceHandlerData
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("balance")]
        public double Balance { get; set; }
    }
}