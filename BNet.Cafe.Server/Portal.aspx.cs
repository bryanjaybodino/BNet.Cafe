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
            // Redirect unauthenticated users
            if (userSession.count == 0)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Fetch account info using current email session
                string email = userSession.user_email;
                DataTable userTable = userRepo.GetByEmail(email);

                if (userTable != null && userTable.Rows.Count > 0)
                {
                    DataRow userRow = userTable.Rows[0];
                    string userId = userRow["DBId"].ToString();
                    string name = userRow["DBName"].ToString();

                    // Set user interface headers
                    litUserName.Text = Server.HtmlEncode(name);
                    litUserEmail.Text = Server.HtmlEncode(email);
                    if (!string.IsNullOrEmpty(name))
                    {
                        litAvatar.Text = name.Substring(0, 1).ToUpper();
                    }

                    // Load duration balance and transaction logs
                    HiddenField_UserId.Value = userId;

                    double totalMinutes = balanceRepo.GetBalanceByUserId(HiddenField_UserId.Value);
                    litTotalMinutes.Text = totalMinutes.ToString();

                    // Format total duration into readable text (hours and minutes)
                    TimeSpan timeSpan = TimeSpan.FromMinutes(totalMinutes);
                    if (timeSpan.TotalHours >= 1)
                    {
                        litFormattedTime.Text = $"{(int)timeSpan.TotalHours} hr {(timeSpan.Minutes > 0 ? timeSpan.Minutes + " min" : "")}";
                    }
                    else
                    {
                        litFormattedTime.Text = $"{totalMinutes} min";
                    }
                }
            }
            // Fetch history records from the Balances repository queries
            DataTable data = balanceRepo.GetAll("", HiddenField_UserId.Value, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);
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