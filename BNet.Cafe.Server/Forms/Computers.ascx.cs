using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.Forms.Modals;
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
            var count = computers.Count();
            Label_Total.Text = count.Total;
            Label_Offline.Text = count.Offline;    
            Label_Occupied.Text = count.Occupied;
            Label_Available.Text = count.Available;

            var data = computers.GetAll(TextBox_Search.Text, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);

            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            for (int i = 0; i < GridViewTable.Rows.Count; i++)
            {
                Label Label_DBComputerName = (Label)GridViewTable.Rows[i].FindControl("Label_DBComputerName");
                Label Label_IPAddress = (Label)GridViewTable.Rows[i].FindControl("Label_IPAddress");
                Label Label_TimeStart = (Label)GridViewTable.Rows[i].FindControl("Label_TimeStart");
                Label Label_TimeEnd = (Label)GridViewTable.Rows[i].FindControl("Label_TimeEnd");
                Label Label_TotalHours = (Label)GridViewTable.Rows[i].FindControl("Label_TotalHours");
                Label Label_Status = (Label)GridViewTable.Rows[i].FindControl("Label_Status");
                Label Label_Billing = (Label)GridViewTable.Rows[i].FindControl("Label_Billing");
                Panel Panel_Logout = (Panel)GridViewTable.Rows[i].FindControl("Panel_Logout");
                Panel Panel_ManageRental = (Panel)GridViewTable.Rows[i].FindControl("Panel_ManageRental");
                Panel Panel_Transfer = (Panel)GridViewTable.Rows[i].FindControl("Panel_Transfer");
                var fetchData = liveData.FirstOrDefault(x => x.ClientName == Label_DBComputerName.Text);

                if (fetchData != null)
                {
                    if (!string.IsNullOrEmpty(fetchData.TimeStart) && DateTime.TryParse(fetchData.TimeStart, out DateTime start))
                    {
                        DateTime.TryParse(fetchData.TimeEnd, out DateTime end);

                        TimeSpan duration = end - start;
                        int hours = (int)duration.TotalHours;
                        int minutes = duration.Minutes;
                        bool isOpenTime = (hours > 100000);
                        bool isAdministrator = (hours == 100);

                        if (isOpenTime && !isAdministrator)
                        {
                            duration = TimeService.Get() - start;
                            hours = (int)duration.TotalHours;
                            minutes = duration.Minutes;
                        }

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

                        Label_IPAddress.Text = fetchData.IPAddress;
                        Label_TimeStart.Text = start.ToString("MMM dd – hh:mm tt");
                        Label_TimeEnd.Text = isOpenTime ? "∞" : end.ToString("MMM dd – hh:mm tt");
                        Label_Status.Text = "Occupied";
                        Label_Status.CssClass = "bnet-badge-status red";
                        Label_Billing.Text = $"₱{billing:N2}";
                        Panel_Logout.Visible = true;
                        Panel_ManageRental.Visible = true;
                        Panel_Transfer.Visible = true;


                        if (isAdministrator)
                        {
                            Label_Status.CssClass = "bnet-badge-status yellow";
                            Label_Status.Text = "Administrator";
                            Label_TimeStart.Text = "--";
                            Label_TimeEnd.Text = "--";
                            Label_Billing.Text = "--";
                            Label_TotalHours.Text = "--";
                            Panel_Logout.Visible = false;
                            Panel_ManageRental.Visible = false;
                            Panel_Transfer.Visible = false;
                        }
                    }
                    else
                    {
                        Label_IPAddress.Text = fetchData.IPAddress;
                        Panel_Logout.Visible = false;
                        Panel_ManageRental.Visible = true;
                        Panel_Transfer.Visible = false;
                        SetDefaultUiState("-", "Available", "bnet-badge-status green");
                    }
                }
                else
                {
                    Panel_Logout.Visible = false;
                    Panel_ManageRental.Visible = false;
                    Panel_Transfer.Visible = false;
                    SetDefaultUiState("-", "Offline", "bnet-badge-status gray");
                }
                void SetDefaultUiState(string textValue, string statusText, string cssClass)
                {
                    Label_TimeStart.Text = textValue;
                    Label_TimeEnd.Text = textValue;
                    Label_TotalHours.Text = textValue;
                    Label_Billing.Text = textValue;
                    Label_Status.Text = statusText;
                    Label_Status.CssClass = cssClass;
                }

            }
        }



        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }

        protected void LinkButton_Refresh_Click(object sender, EventArgs e)
        {
            // Explicitly handles the refresh postback so Web Forms does not re-fire previous events
        }
    }
}