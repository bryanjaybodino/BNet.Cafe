using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Ashx
{
    public class PricingRateItem
    {
        [JsonProperty("DBId")]
        public int Id { get; set; }

        [JsonProperty("DBCustomerType")]
        public string CustomerType { get; set; }

        [JsonProperty("DBMinutes")]
        public int Minutes { get; set; }

        [JsonProperty("DBPrice")]
        public double Price { get; set; }

        [JsonProperty("DBIsDeleted")]
        public bool IsDeleted { get; set; }
    }
}
