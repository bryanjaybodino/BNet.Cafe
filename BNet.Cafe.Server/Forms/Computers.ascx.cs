using BNet.Cafe.Server.Ashx;
using BNet.Cafe.Server.Forms.Modals;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Computers : System.Web.UI.UserControl
    {
        Repositories.Computers computers = new Repositories.Computers();
        Repositories.Rentals rentals = new Repositories.Rentals();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            var count = computers.GetCount();
            Label_Total.Text = count.Total;
            Label_Offline.Text = count.Offline;
            Label_Occupied.Text = count.Occupied;
            Label_Available.Text = count.Available;

            // 1. Get raw database computer records without pagination limit first
            DataTable rawData = computers.GetAll(TextBox_Search.Text);

            // 2. Create the unified combined DataTable
            DataTable dtCombined = CreateCombinedDataTable();

            // 3. Fetch live client data
            ClientData clientData = new ClientData();
            var liveData = clientData.FetchData();

            // 4. Process each database row and calculate live status
            foreach (DataRow row in rawData.Rows)
            {
                DataRow newRow = dtCombined.NewRow();
                string dbId = row["DBId"].ToString();
                string computerName = row["DBComputerName"].ToString();

                newRow["DBId"] = dbId;
                newRow["DBComputerName"] = computerName;

                var fetchData = liveData.FirstOrDefault(x => x.ClientName == computerName);

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
                        double billing = 0;

                        if (isOpenTime && !isAdministrator)
                        {
                            duration = TimeService.Get() - start;
                            hours = (int)duration.TotalHours;
                            minutes = duration.Minutes;
                            billing = CalculateAmountFromDurationHandler.CalculatePrice((int)duration.TotalMinutes);
                        }
                        else
                        {
                            var rentalData = rentals.GetByComputerId(dbId);
                            if (rentalData.Rows.Count > 0)
                            {
                                billing = Convert.ToDouble(rentalData.Rows[0]["DBAmount"].ToString());
                            }
                            else
                            {
                                billing = CalculateAmountFromDurationHandler.CalculatePrice((int)duration.TotalMinutes);
                            }
                        }

                        string hrLabel = hours == 1 ? "hr" : "hrs";
                        string minLabel = minutes == 1 ? "min" : "mins";
                        string totalHoursText = "0 mins";

                        if (hours > 0 && minutes > 0)
                            totalHoursText = $"{hours} {hrLabel} {minutes} {minLabel}";
                        else if (hours > 0)
                            totalHoursText = $"{hours} {hrLabel}";
                        else if (minutes > 0)
                            totalHoursText = $"{minutes} {minLabel}";
                        bool isPaused = string.Equals(fetchData.IsPaused, "TRUE", StringComparison.OrdinalIgnoreCase);

                        newRow["IPAddress"] = fetchData.IPAddress;
                        newRow["TimeStart"] = start.ToString("MMM dd – hh:mm tt");
                        newRow["TimeEnd"] = isOpenTime ? "∞" : end.ToString("MMM dd – hh:mm tt");
                        newRow["TotalHours"] = totalHoursText;

                        newRow["Status"] = isPaused ? "Paused" : "Occupied";
                        newRow["StatusCssClass"] = isPaused ? "bnet-badge-status yellow" : "bnet-badge-status red";
                        newRow["Billing"] = isPaused ? "--" : $"₱{billing:N2}";

                        newRow["IsLogoutVisible"] = true;
                        newRow["IsManageRentalVisible"] = true;
                        newRow["IsTransferVisible"] = true;
                        newRow["IsPauseResumeVisible"] = true;

                        if (isAdministrator)
                        {
                            newRow["Status"] = "Administrator";
                            newRow["StatusCssClass"] = "bnet-badge-status yellow";
                            newRow["TimeStart"] = "--";
                            newRow["TimeEnd"] = "--";
                            newRow["Billing"] = "--";
                            newRow["TotalHours"] = "--";
                            newRow["IsLogoutVisible"] = false;
                            newRow["IsManageRentalVisible"] = false;
                            newRow["IsTransferVisible"] = false;
                            newRow["IsPauseResumeVisible"] = false;
                        }
                    }
                    else
                    {
                        newRow["IPAddress"] = fetchData.IPAddress;
                        SetDefaultRowState(newRow, "-", "Available", "bnet-badge-status green");
                        newRow["IsLogoutVisible"] = false;
                        newRow["IsManageRentalVisible"] = true;
                        newRow["IsTransferVisible"] = false;
                        newRow["IsPauseResumeVisible"] = false;
                    }
                }
                else
                {
                    SetDefaultRowState(newRow, "-", "Offline", "bnet-badge-status gray");
                    newRow["IPAddress"] = "-";
                    newRow["IsLogoutVisible"] = false;
                    newRow["IsManageRentalVisible"] = false;
                    newRow["IsTransferVisible"] = false;
                    newRow["IsPauseResumeVisible"] = false;
                }

                dtCombined.Rows.Add(newRow);
            }

            // 5. Apply Status or Search Filtering directly on the combined DataTable
            DataView dv = dtCombined.DefaultView;
            List<string> filters = new List<string>();

            if (!string.IsNullOrEmpty(TextBox_Search.Text))
            {
                string search = TextBox_Search.Text.Replace("'", "''");
                filters.Add($"DBComputerName LIKE '%{search}%'");
            }

            if (DropDownList_Status.SelectedValue != "All")
            {
                string status = DropDownList_Status.SelectedValue.Replace("'", "''");
                filters.Add($"Status = '{status}'");
            }

            if (filters.Count > 0)
            {
                dv.RowFilter = string.Join(" AND ", filters);
            }

            // 6. Bind the processed DataTable to GridView
            GridViewTemplateService.SetGridView(GridViewTable, dv.ToTable(), Panel_Pagination);
        }

        private DataTable CreateCombinedDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("DBId", typeof(string));
            dt.Columns.Add("DBComputerName", typeof(string));
            dt.Columns.Add("IPAddress", typeof(string));
            dt.Columns.Add("TimeStart", typeof(string));
            dt.Columns.Add("TimeEnd", typeof(string));
            dt.Columns.Add("TotalHours", typeof(string));
            dt.Columns.Add("Status", typeof(string));
            dt.Columns.Add("StatusCssClass", typeof(string));
            dt.Columns.Add("Billing", typeof(string));
            dt.Columns.Add("IsLogoutVisible", typeof(bool));
            dt.Columns.Add("IsManageRentalVisible", typeof(bool));
            dt.Columns.Add("IsTransferVisible", typeof(bool));
            dt.Columns.Add("IsPauseResumeVisible", typeof(bool));
            return dt;
        }

        private void SetDefaultRowState(DataRow row, string textValue, string statusText, string cssClass)
        {
            row["TimeStart"] = textValue;
            row["TimeEnd"] = textValue;
            row["TotalHours"] = textValue;
            row["Billing"] = textValue;
            row["Status"] = statusText;
            row["StatusCssClass"] = cssClass;
        }

        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }

    }
}