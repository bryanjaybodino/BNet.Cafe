using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class SeatMap : System.Web.UI.UserControl
    {
        Repositories.Computers computers = new Repositories.Computers();

        public class SeatMapViewModel
        {
            public string DBId { get; set; }
            public string DBComputerName { get; set; }
            public int PosX { get; set; }
            public int PosY { get; set; }
            public string StatusText { get; set; }
            public string StatusClass { get; set; }
        }

        // DTO for Deserializing the JSON Payload from HiddenField_Positions
        public class SeatMapPositionDto
        {
            public string id { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            LoadComputerSeats();
        }

        private void LoadComputerSeats()
        {
            var dataTable = computers.GetAll(); // Get all computers
            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            List<SeatMapViewModel> seatList = new List<SeatMapViewModel>();

            foreach (DataRow row in dataTable.Rows)
            {
                string dbId = row["DBId"].ToString();
                string compName = row["DBComputerName"].ToString();
                int x = row["DBPosX"] != DBNull.Value ? Convert.ToInt32(row["DBPosX"]) : 20;
                int y = row["DBPosY"] != DBNull.Value ? Convert.ToInt32(row["DBPosY"]) : 20;

                var live = liveData.FirstOrDefault(xData => xData.ClientName == compName);

                string statusText = "Offline";
                string statusClass = "offline";

                if (live != null)
                {
                    if (!string.IsNullOrEmpty(live.TimeStart))
                    {
                        if (live.IsPaused.ToUpper() == "TRUE")
                        {
                            statusText = "Paused";
                            statusClass = "paused";
                        }
                        else
                        {
                            statusText = "Occupied";
                            statusClass = "occupied";
                        }
                    }
                    else
                    {
                        statusText = "Available";
                        statusClass = "available";
                    }
                }

                seatList.Add(new SeatMapViewModel
                {
                    DBId = dbId,
                    DBComputerName = compName,
                    PosX = x,
                    PosY = y,
                    StatusText = statusText,
                    StatusClass = statusClass
                });
            }

            Repeater_Computers.DataSource = seatList;
            Repeater_Computers.DataBind();
        }

        protected void Button_SaveLayout_Click(object sender, EventArgs e)
        {
            string rawData = HiddenField_Positions.Value;

            if (!string.IsNullOrEmpty(rawData))
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<SeatMapPositionDto> positions = serializer.Deserialize<List<SeatMapPositionDto>>(rawData);

                if (positions != null && positions.Count > 0)
                {
                    bool allSavedSuccessfully = true;

                    foreach (var pos in positions)
                    {
                        // Save each seat's updated coordinates to the database
                        bool isSuccess = computers.UpdatePosition(pos.id, pos.x.ToString(), pos.y.ToString());
                        if (!isSuccess)
                        {
                            allSavedSuccessfully = false;
                        }
                    }

                    if (allSavedSuccessfully)
                    {
                        AlertService.ShowAlert(UpdatePanel1, "Floor plan layout updated successfully.", "success");
                    }
                    else
                    {
                        AlertService.ShowAlert(UpdatePanel1, "Failed to update some computer positions. Please try again.", "error");
                    }
                }
                else
                {
                    AlertService.ShowAlert(UpdatePanel1, "No layout changes were detected to save.", "warning");
                }
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Unable to save layout. Position data is missing.", "error");
            }

            // Reload seats to reflect changes across the UI
            LoadComputerSeats();
        }
    }
}