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
            public string IPAddress { get; set; }
            public string IsPaused { get; set; }
        }
        public List<ClientData.ClientTimeout> FetchData()
        {
            try
            {
                string apiUrl = $"http://{Services.NetworkUtility.GetLocalIPAddress()}:2050/text";
                using (WebClient client = new WebClient())
                {
                    // Synchronously download the JSON string from the API
                    string jsonResult = client.DownloadString(apiUrl);

                    // Deserialize the JSON array using your model class
                    List<ClientData.ClientTimeout> clientList = JsonConvert.DeserializeObject<List<ClientData.ClientTimeout>>(jsonResult);

                    return clientList;
                }
            }
            catch
            {
                return new List<ClientData.ClientTimeout>();
            }

        }

        public string FetchStatus(string clientName)
        {
            try
            {
                string apiUrl = $"http://{Services.NetworkUtility.GetLocalIPAddress()}:2050/text";
                using (WebClient client = new WebClient())
                {
                    string jsonResult = client.DownloadString(apiUrl);
                    List<ClientData.ClientTimeout> clientList = JsonConvert.DeserializeObject<List<ClientData.ClientTimeout>>(jsonResult);

                    if (clientList == null)
                        return "Offline";

                    var clientData = clientList.FirstOrDefault(x => x.ClientName == clientName);

                    if (clientData != null)
                    {
                        if (!string.IsNullOrEmpty(clientData.TimeStart) && DateTime.TryParse(clientData.TimeStart, out DateTime start))
                        {
                            DateTime.TryParse(clientData.TimeEnd, out DateTime end);
                            TimeSpan duration = end - start;
                            int hours = (int)duration.TotalHours;
                            bool isAdministrator = (hours == 100);

                            if (isAdministrator)
                            {
                                return "Administrator";
                            }
                            else
                            {
                                if (clientData.IsPaused.ToUpper() == "TRUE")
                                {
                                    return "Pause";
                                }
                                else
                                {
                                    return "Occupied";
                                }                       
                            }
                        }

                        return "Available";
                    }

                    return "Offline";
                }
            }
            catch
            {
                return "Offline";
            }
        }
    }
}