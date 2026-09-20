using BNet.Cafe.Server.Databases.Tables;
using BNet.Cafe.Server.Forms;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using BNet.Cafe.Server.Sessions;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server
{
    public partial class Portal : Page
    {
        private readonly User userSession = new User();
        private readonly Repositories.Users userRepo = new Repositories.Users();
        private readonly Balances balanceRepo = new Balances();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Decrypt and check parameter safely
                bool isLoginQuery = false;
                if (!string.IsNullOrEmpty(Request.QueryString["UserId"]))
                {
                    string encryptedUserId = Request.QueryString["UserId"];
                    string userId = SecuredDataService.Decrypted(encryptedUserId);
                    isLoginQuery = !string.IsNullOrWhiteSpace(userId);

                    if (isLoginQuery)
                    {
                        var userData = userRepo.GetById(userId);
                        for (int i = 0; i < userData.Rows.Count; i++)
                        {
                            string _email = userData.Rows[i]["DBEmail"].ToString();
                            string _password = userData.Rows[i]["DBPassword"].ToString();
                            string _role = userData.Rows[i]["DBRole"].ToString();
                            userSession.createCookies(_email, _password, _role);
                        }
                        Response.Redirect("~/Portal.aspx");
                        return;
                    }
                }

                // Redirect if unauthenticated AND NOT coming from a valid login query
                if (userSession.count == 0 && !isLoginQuery)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                // Fetch account info using current email session
                string email = userSession.user_email;
                DataTable userTable = userRepo.GetByEmail(email);

                if (userTable != null && userTable.Rows.Count > 0)
                {
                    DataRow userRow = userTable.Rows[0];
                    string userId = userRow["DBId"].ToString();
                    string name = userRow["DBName"].ToString();
                    double accountBalanceMinutes = Convert.ToDouble(userRow["DBTotalDuration"]);

                    HiddenField_UserId.Value = userId;

                    // Set user info
                    litUserName.Text = Server.HtmlEncode(name);
                    litUserEmail.Text = Server.HtmlEncode(email);
                    if (!string.IsNullOrEmpty(name))
                    {
                        litAvatar.Text = name.Substring(0, 1).ToUpper();
                    }

                    // 1. Account / Banked Balance Card (Top-ups while playing stay here)
                    litAccountBalanceMins.Text = accountBalanceMinutes.ToString();
                    litAccountBalanceTime.Text = FormatDuration(accountBalanceMinutes);
                }
            }

            // Single call to fetch transaction logs and bind GridView
            DataTable data = balanceRepo.GetAll("", HiddenField_UserId.Value, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);

            double activeSessionMinutes = 0;
            bool foundSession = false;

            // Loop directly through GridView rows to format time and check active session using FindControl
            for (int i = 0; i < GridViewTable.Rows.Count; i++)
            {
                GridViewRow row = GridViewTable.Rows[i];

                // Format Time Label
                try
                {
                    Label Label_DBTimeCreated = (Label)row.FindControl("Label_DBTimeCreated");
                    if (Label_DBTimeCreated != null && !string.IsNullOrEmpty(Label_DBTimeCreated.Text))
                    {
                        Label_DBTimeCreated.Text = Convert.ToDateTime(Label_DBTimeCreated.Text).ToString("hh:mm tt");
                    }
                }
                catch { }

                // Evaluate Active Session State from rendered GridView controls
                if (!foundSession)
                {
                    Label Label_DBDescription = (Label)row.FindControl("Label_DBDescription");
                    string description = Label_DBDescription != null ? Label_DBDescription.Text : "";

                    if (description.Contains("LOGGING-OUT"))
                    {
                        activeSessionMinutes = 0;
                        foundSession = true; // Session ended
                    }
                    else if (description.Contains("LOGGING-IN"))
                    {
                        Label Label_DBDuration = (Label)row.FindControl("Label_DBDuration");
                        string durationText = Label_DBDuration != null ? Label_DBDuration.Text : "";

                        // Extract digits/numbers from string like "-3 hrs 49 mins" or "-229"
                        if (double.TryParse(System.Text.RegularExpressions.Regex.Match(durationText, @"\d+").Value, out double duration))
                        {
                            activeSessionMinutes = duration;
                        }
                        foundSession = true; // Active session entry found
                    }
                }
            }

            // Update active session cards
            litActiveSessionMins.Text = activeSessionMinutes.ToString();
            litActiveSessionTime.Text = FormatDuration(activeSessionMinutes);
        }

        private string FormatDuration(double totalMinutes)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(totalMinutes);
            if (timeSpan.TotalHours >= 1)
            {
                int hours = (int)timeSpan.TotalHours;
                int mins = timeSpan.Minutes;
                return mins > 0 ? $"{hours} hr {mins} min" : $"{hours} hr";
            }
            return $"{totalMinutes} mins";
        }

        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            userSession.RemoveCookies();
            Response.Redirect("~/Login.aspx?logout=true");
        }
    }
}