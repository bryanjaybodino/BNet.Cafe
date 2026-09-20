using Newtonsoft.Json;

namespace BNet.Cafe.Client.Ashx
{
    public class BalanceData
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("balance")]
        public double Balance { get; set; }
    }

    public class BalanceApiResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public BalanceData Data { get; set; }
    }
}