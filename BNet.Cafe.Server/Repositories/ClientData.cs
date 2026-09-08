using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace BNet.Cafe.Server.Repositories
{
    public class ClientData
    {
        public class ClientTimeout
        {
            public string ClientName { get; set; }
            public string TimeStart { get; set; }
            public string TimeEnd { get; set; }
        }
        public List<ClientData.ClientTimeout> FetchData()
        {
            string apiUrl = "http://localhost:2050/text";
            using (WebClient client = new WebClient())
            {
                // Synchronously download the JSON string from the API
                string jsonResult = client.DownloadString(apiUrl);

                // Deserialize the JSON array using your model class
                List<ClientData.ClientTimeout> clientList = JsonConvert.DeserializeObject<List<ClientData.ClientTimeout>>(jsonResult);

                return clientList;
            }

        }
    }
}