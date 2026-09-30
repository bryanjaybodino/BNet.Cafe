using Newtonsoft.Json;
using System.Collections.Generic;

namespace BNet.Cafe.Client.Ashx
{
    public class GetWallpaperCountHandlerData
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public WallpaperData Data { get; set; }
    }

    public class WallpaperData
    {
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("fileNames")]
        public List<string> FileNames { get; set; }
    }
}