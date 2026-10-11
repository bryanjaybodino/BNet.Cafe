using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;

namespace BNet.Cafe.Server.Forms
{
    public partial class Config : System.Web.UI.UserControl
    {
        private readonly ClientConfig clientConfigRepository = new ClientConfig();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateTimeZones();
                LoadConfiguration();
            }
        }

        private void PopulateTimeZones()
        {
            DropDownList_TimeZone.Items.Clear();
            foreach (TimeZoneInfo timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                string text = string.Format("({0}) {1}", timeZone.BaseUtcOffset, timeZone.DisplayName);
                string value = timeZone.Id;
                DropDownList_TimeZone.Items.Add(new System.Web.UI.WebControls.ListItem(text.ToUpper(), value.ToUpper()));
            }
        }

        private void LoadConfiguration()
        {
            DataTable dt = clientConfigRepository.GetAll();

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                CheckBox_AccountCreationAllowed.Checked = row["DBAccountCreationAllowed"].ToString().ToUpper() == "TRUE";
                TextBox_AutoShutDownInterval.Text = row["DBAutoShutDownInterval"].ToString();
                CheckBox_DesktopSlideShow.Checked = row["DBDesktopSlideShow"].ToString().ToUpper() == "TRUE";
                CheckBox_ResetShutdownCountdown.Checked = row["DBResetShutdownCountdown"].ToString().ToUpper() == "TRUE";

                // Assuming you add a DBTimeZone column to your table. 
                // Fallback to local system time zone if column data is empty.
                string savedTimeZone = row.Table.Columns.Contains("DBTimeZone") ? row["DBTimeZone"].ToString() : "";
                if (!string.IsNullOrEmpty(savedTimeZone) && DropDownList_TimeZone.Items.FindByValue(savedTimeZone) != null)
                {
                    DropDownList_TimeZone.SelectedValue = savedTimeZone;
                }
                else
                {
                    try { DropDownList_TimeZone.SelectedValue = TimeZoneInfo.Local.Id.ToUpper(); } catch { }
                }
            }
            else
            {
                // Default fallback values if database table is empty
                CheckBox_AccountCreationAllowed.Checked = true;
                TextBox_AutoShutDownInterval.Text = "300";
                CheckBox_DesktopSlideShow.Checked = false;
                CheckBox_ResetShutdownCountdown.Checked = false;
                try { DropDownList_TimeZone.SelectedValue = TimeZoneInfo.Local.Id.ToUpper(); } catch { }
            }
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string accountCreationAllowed = CheckBox_AccountCreationAllowed.Checked ? "TRUE" : "FALSE";
            string autoShutDownInterval = string.IsNullOrWhiteSpace(TextBox_AutoShutDownInterval.Text) ? "300" : TextBox_AutoShutDownInterval.Text.Trim();
            string desktopSlideShow = CheckBox_DesktopSlideShow.Checked ? "TRUE" : "FALSE";
            string resetShutdownCountdown = CheckBox_ResetShutdownCountdown.Checked ? "TRUE" : "FALSE";
            string timeZoneId = DropDownList_TimeZone.SelectedValue;

            DataTable dt = clientConfigRepository.GetAll();

            bool isSuccess;
            if (dt != null && dt.Rows.Count > 0)
            {
                string id = dt.Rows[0]["DBId"].ToString();
                isSuccess = clientConfigRepository.Update(
                    id,
                    accountCreationAllowed,
                    autoShutDownInterval,
                    desktopSlideShow,
                    resetShutdownCountdown,
                    timeZoneId
                );
            }
            else
            {
                isSuccess = clientConfigRepository.Create(
                    accountCreationAllowed,
                    autoShutDownInterval,
                    desktopSlideShow,
                    resetShutdownCountdown
                );
            }

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Client configuration saved successfully.", "success");
                ClientData clientData = new ClientData();
                var targetList = clientData.FetchData();
                foreach (var target in targetList)
                {
                    RemoteMessagingService.RefreshPC(UpdatePanel1, target.ClientName);
                }
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to save client configuration.", "error");
            }
        }
    }
}