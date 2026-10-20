using System;
using System.Data;
using System.Web.UI.WebControls;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms
{
    public partial class PointsSettings : System.Web.UI.UserControl
    {
        private readonly Repositories.PointsRates pointsRepository = new Repositories.PointsRates();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            LoadPointsList();
        }

        private void LoadPointsList()
        {
            DataTable dt = pointsRepository.GetAll(DropDownList_CustomerType.SelectedValue);
            GridView_Points.DataSource = dt;
            GridView_Points.DataBind();
            Label_TotalRules.Text = GridView_Points.Rows.Count.ToString();
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string id = HiddenField_PointId.Value;
            string customerType = DropDownList_CustomerType.SelectedValue;
            string points = TextBox_Points.Text.Trim();

            // Parse Hours and Minutes inputs safely
            int.TryParse(TextBox_Hours.Text.Trim(), out int hours);
            int.TryParse(TextBox_Minutes_Only.Text.Trim(), out int minutesOnly);

            // Calculate total minutes
            int totalMinutes = (hours * 60) + minutesOnly;
            string minutes = totalMinutes.ToString();

            // Validation
            if (string.IsNullOrEmpty(customerType) || totalMinutes <= 0 || string.IsNullOrEmpty(points))
            {
                AlertService.ShowAlert(UpdatePanel1, "Please fill in all required fields with a valid duration and points value.", "warning");
                return;
            }

            bool isSuccess;

            if (id == "0" || string.IsNullOrEmpty(id))
            {
                isSuccess = pointsRepository.Create(customerType, minutes, points);
            }
            else
            {
                isSuccess = pointsRepository.Update(id, customerType, minutes, points);
            }

            if (isSuccess)
            {
                ClearForm();
                string actionMsg = (id == "0" || string.IsNullOrEmpty(id)) ? "added" : "updated";
                AlertService.ShowAlert(UpdatePanel1, $"Points rule {actionMsg} successfully.", "success");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to save points rule or it already exists. Please try again.", "error");
            }
        }

        protected void LinkButton_Cancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            HiddenField_PointId.Value = "0";
            TextBox_Hours.Text = string.Empty;
            TextBox_Minutes_Only.Text = string.Empty;
            TextBox_Points.Text = string.Empty;
            LinkButton_Submit.Text = "<i class=\"fa fa-save\"></i> Save Rule";
        }

        /// <summary>
        /// Format minutes to hours and minutes display
        /// </summary>
        protected string FormatDuration(int totalMinutes)
        {
            if (totalMinutes <= 0) return "0 mins";

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            if (hours > 0 && minutes > 0)
                return $"{hours}h {minutes}m";
            else if (hours > 0)
                return $"{hours} hr" + (hours > 1 ? "s" : "");
            else
                return $"{minutes} min" + (minutes > 1 ? "s" : "");
        }
    }
}