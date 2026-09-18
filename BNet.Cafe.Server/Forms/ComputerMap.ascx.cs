using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.Repositories;

namespace BNet.Cafe.Server.Forms
{
    public partial class ComputerMap : UserControl
    {
        Repositories.Computers computers = new Repositories.Computers();

        public class ComputerSeatViewModel
        {
            public string DBId { get; set; }
            public string DBComputerName { get; set; }
            public int PosX { get; set; }
            public int PosY { get; set; }
            public string StatusText { get; set; }
            public string StatusClass { get; set; }
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

            List<ComputerSeatViewModel> seatList = new List<ComputerSeatViewModel>();

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

                seatList.Add(new ComputerSeatViewModel
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
            // Raw JSON format: [{"id":"1","x":120,"y":50},{"id":"2","x":240,"y":50}]
            if (!string.IsNullOrEmpty(rawData))
            {
                // Update database coordinates using Repository
                // computers.UpdatePositions(rawData);
            }
        }
    }
}