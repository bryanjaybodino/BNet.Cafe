using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Ashx
{
    /// <summary>
    /// Summary description for GetComputerHandler
    /// </summary>
    public class GetComputerHandler : IHttpHandler
    {
        private readonly Repositories.Computers _computersRepository = new Repositories.Computers();
        private readonly Repositories.ClientData _clientDataRepository = new Repositories.ClientData();
        private readonly Repositories.Rentals _rentalsRepository = new Repositories.Rentals();
        private readonly Repositories.Users _usersRepository = new Repositories.Users();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                string computerId = context.Request["computerId"];
                string computerName = context.Request["computerName"];

                DataTable rawData = _computersRepository.GetAll();
                var liveData = _clientDataRepository.FetchData();

                var computerList = new List<GetComputerHandlerData>();

                foreach (DataRow row in rawData.Rows)
                {
                    string dbId = row["DBId"].ToString();
                    string dbComputerName = row["DBComputerName"].ToString();

                    if (!string.IsNullOrWhiteSpace(computerId) && dbId != computerId) continue;
                    if (!string.IsNullOrWhiteSpace(computerName) && !dbComputerName.Equals(computerName, StringComparison.OrdinalIgnoreCase)) continue;

                    var fetchData = liveData.FirstOrDefault(x => x.ClientName == dbComputerName);

                    string ipAddress = "-";
                    string timeStart = "";
                    string timeEnd = "";
                    string isPaused = "FALSE";
                    string userType = "GUEST";

                    if (fetchData != null)
                    {
                        ipAddress = fetchData.IPAddress;
                        timeStart = fetchData.TimeStart ?? "";
                        timeEnd = fetchData.TimeEnd ?? "";
                        isPaused = fetchData.IsPaused ?? "FALSE";
                    }

                    string status = _clientDataRepository.FetchStatus(dbComputerName);

                    var rentalData = _rentalsRepository.GetByComputerId(dbId);
                    if (rentalData != null && rentalData.Rows.Count > 0)
                    {
                        string userId = rentalData.Rows[0]["DBUserId"]?.ToString();
                        if (!string.IsNullOrEmpty(userId))
                        {
                            var userData = _usersRepository.GetById(userId);
                            if (userData != null && userData.Rows.Count > 0)
                            {
                                userType = userData.Rows[0]["DBRole"]?.ToString() ?? "GUEST";
                            }
                        }
                    }

                    computerList.Add(new GetComputerHandlerData
                    {
                        Id = dbId,
                        ClientName = dbComputerName,
                        IPAddress = ipAddress,
                        TimeStart = timeStart,
                        TimeEnd = timeEnd,
                        IsPaused = isPaused,
                        UserType = userType,
                        Status = status
                    });
                }

                SendJsonResponse(context, true, "Computers retrieved successfully.", computerList);
            }
            catch (Exception ex)
            {
                SendJsonResponse(context, false, $"Server error: {ex.Message}", null);
            }
        }

        private void SendJsonResponse(HttpContext context, bool success, string message, object data = null)
        {
            string jsonResponse = JsonConvert.SerializeObject(data);
            context.Response.Write(jsonResponse);
        }

        public bool IsReusable => false;

        public class GetComputerHandlerData
        {
            [JsonProperty("Id")]
            public string Id { get; set; }

            [JsonProperty("ClientName")]
            public string ClientName { get; set; }

            [JsonProperty("IPAddress")]
            public string IPAddress { get; set; }

            [JsonProperty("TimeStart")]
            public string TimeStart { get; set; }

            [JsonProperty("TimeEnd")]
            public string TimeEnd { get; set; }

            [JsonProperty("IsPaused")]
            public string IsPaused { get; set; }

            [JsonProperty("UserType")]
            public string UserType { get; set; }

            [JsonProperty("Status")]
            public string Status { get; set; }
        }
    }
}