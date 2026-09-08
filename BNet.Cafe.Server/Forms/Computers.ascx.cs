using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Computers : System.Web.UI.UserControl
    {
        Repositories.Computers computers = new Repositories.Computers();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            var data = computers.GetAll(TextBox_Search.Text, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            for (int i = 0; i < GridViewTable.Rows.Count; i++)
            {
                Label Label_DBComputerName = (Label)GridViewTable.Rows[i].FindControl("Label_DBComputerName");
                Label Label_TimeStart = (Label)GridViewTable.Rows[i].FindControl("Label_TimeStart");
                Label Label_TimeEnd = (Label)GridViewTable.Rows[i].FindControl("Label_TimeEnd");
                Label Label_TotalHours = (Label)GridViewTable.Rows[i].FindControl("Label_TotalHours");
                Label Label_Status = (Label)GridViewTable.Rows[i].FindControl("Label_Status");
                Label Label_Billing = (Label)GridViewTable.Rows[i].FindControl("Label_Billing");
                var fetchData = liveData.FirstOrDefault(x => x.ClientName == Label_DBComputerName.Text);

                if (fetchData != null)
                {
                    if (!string.IsNullOrEmpty(fetchData.TimeStart) && DateTime.TryParse(fetchData.TimeStart, out DateTime start))
                    {
                        DateTime.TryParse(fetchData.TimeEnd, out DateTime end);

                        TimeSpan duration = end - start;
                        int hours = (int)duration.TotalHours;
                        int minutes = duration.Minutes;


                        double billing = CalculateRentalPrice.CalculatePrice((int)duration.TotalMinutes);



                        string hrLabel = hours == 1 ? "hr" : "hrs";
                        string minLabel = minutes == 1 ? "min" : "mins";

                        if (hours > 0 && minutes > 0)
                        {
                            Label_TotalHours.Text = $"{hours} {hrLabel} {minutes} {minLabel}";
                        }
                        else if (hours > 0)
                        {
                            Label_TotalHours.Text = $"{hours} {hrLabel}";
                        }
                        else if (minutes > 0)
                        {
                            Label_TotalHours.Text = $"{minutes} {minLabel}"; // Handles "1 min" and "15 mins"
                        }
                        else
                        {
                            Label_TotalHours.Text = "0 mins";
                        }

                        Label_TimeStart.Text = start.ToString("MMM dd, yyyy – hh:mm tt");
                        Label_TimeEnd.Text = end.ToString("MMM dd, yyyy – hh:mm tt");
                        Label_Status.Text = "Occupied";
                        Label_Status.CssClass = "badge-status red";
                        Label_Billing.Text = $"₱{billing:N2}";  
                    }
                    else
                    {
                        Label_TimeStart.Text = "-";
                        Label_TimeEnd.Text = "-";
                        Label_TotalHours.Text = "-";
                        Label_Billing.Text = "-";
                        Label_Status.Text = "Available";
                        Label_Status.CssClass = "badge-status green";
                    }
                }
                else
                {
                    Label_TimeStart.Text = "-";
                    Label_TimeEnd.Text = "-";
                    Label_TotalHours.Text = "-";
                    Label_Billing.Text = "-";
                    Label_Status.Text = "Offline";
                    Label_Status.CssClass = "badge-status gray";
                }
            }
        }
        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }
    }
}