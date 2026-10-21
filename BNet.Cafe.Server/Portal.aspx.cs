using BNet.Cafe.Server.Databases.Tables;
using BNet.Cafe.Server.Forms;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using BNet.Cafe.Server.Sessions;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Points = BNet.Cafe.Server.Repositories.Points;

namespace BNet.Cafe.Server
{
    public partial class Portal : Page
    {
        private readonly User userSession = new User();
        private readonly Repositories.Users userRepo = new Repositories.Users();
        private readonly Balances balanceRepo = new Balances();
        private readonly Points pointsRepo = new Points();
        private readonly PricingRates pricingRatesRepo = new PricingRates();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FileJsHelpler.BundleAddScripts(ScriptManager1, "Portal/Script");
                bool isLoginQuery = false;
                if (!string.IsNullOrEmpty(Request.QueryString["UserId"]))
                {
                    string encryptedUserId = Request.QueryString["UserId"];
                    string userId = SecuredDataService.Decrypted(encryptedUserId);
                    isLoginQuery = !string.IsNullOrWhiteSpace(userId);

                    if (isLoginQuery && !ConstantData.UserType.Guest.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var userData = userRepo.GetById(userId);
                        for (int i = 0; i < userData.Rows.Count; i++)
                        {
                            string _email = userData.Rows[i]["DBEmail"].ToString();
                            string _password = userData.Rows[i]["DBPassword"].ToString();
                            string _role = userData.Rows[i]["DBRole"].ToString();
                            userSession.createCookies(_email, _password, _role, userId);
                        }

                        Response.Redirect("~/Portal.aspx");
                        return;
                    }
                    else
                    {
                        userSession.RemoveCookies();
                        Response.Redirect("~/ExpiredOrInvalid.aspx");
                        return;
                    }
                }

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
                    string role = userRow["DBRole"].ToString();

                    double accountBalanceMinutes = balanceRepo.GetBalanceByUserId(userId);
                    double totalPointsValue = pointsRepo.GetPointsByUserId(userId);

                    HiddenField_UserId.Value = userId;
                    HiddenField_CustomerType.Value = role;
                    litCustomerType.Text = Server.HtmlEncode(role);

                    litUserName.Text = Server.HtmlEncode(name);
                    litUserEmail.Text = Server.HtmlEncode(email);
                    if (!string.IsNullOrEmpty(name))
                    {
                        litAvatar.Text = name.Substring(0, 1).ToUpper();
                    }

                    litAccountBalanceMins.Text = accountBalanceMinutes.ToString();
                    litAccountBalanceTime.Text = FormatDuration(accountBalanceMinutes);
                    litTotalPoints.Text = totalPointsValue.ToString();
                }
            }

            // 1. Bind Transaction History GridView
            DataTable transactionData = balanceRepo.GetAll("", HiddenField_UserId.Value, "", GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, transactionData, Panel_Pagination);

            // 2. Bind Pricing Rates based on User Role / Customer Type
            string currentCustomerType = string.IsNullOrEmpty(HiddenField_CustomerType.Value) ? "MEMBER" : HiddenField_CustomerType.Value;
            DataTable pricingTable = pricingRatesRepo.GetAll(currentCustomerType);
            RepeaterPricingRates.DataSource = pricingTable;
            RepeaterPricingRates.DataBind();

            // 3. Bind Points History GridView
            DataTable pointsData = pointsRepo.GetAll("", HiddenField_UserId.Value, "", GridViewTemplateService.GetPaginationIndex(GridViewPoints));
            GridViewTemplateService.SetGridView(GridViewPoints, pointsData, Panel_PointsPagination);

            // Calculate active session minutes
            double activeSessionMinutes = 0;
            bool foundSession = false;

            for (int i = 0; i < GridViewTable.Rows.Count; i++)
            {
                GridViewRow row = GridViewTable.Rows[i];
                try
                {
                    Label Label_DBTimeCreated = (Label)row.FindControl("Label_DBTimeCreated");
                    if (Label_DBTimeCreated != null && !string.IsNullOrEmpty(Label_DBTimeCreated.Text))
                    {
                        Label_DBTimeCreated.Text = Convert.ToDateTime(Label_DBTimeCreated.Text).ToString("hh:mm tt");
                    }
                }
                catch { }

                if (!foundSession)
                {
                    Label Label_DBDescription = (Label)row.FindControl("Label_DBDescription");
                    string description = Label_DBDescription != null ? Label_DBDescription.Text : "";

                    if (description.Contains("LOGGING-OUT"))
                    {
                        activeSessionMinutes = 0;
                        foundSession = true;
                    }
                    else if (description.Contains("LOGGING-IN"))
                    {
                        Label Label_DBDuration = (Label)row.FindControl("Label_DBDuration");
                        string durationText = Label_DBDuration != null ? Label_DBDuration.Text : "";

                        if (double.TryParse(System.Text.RegularExpressions.Regex.Match(durationText, @"\d+").Value, out double duration))
                        {
                            activeSessionMinutes = duration;
                        }
                        foundSession = true;
                    }
                }
            }

            litActiveSessionMins.Text = activeSessionMinutes.ToString();
            litActiveSessionTime.Text = FormatDuration(activeSessionMinutes);
        }

        protected void lnkTab_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string command = btn.CommandArgument;

            // Reset tab classes
            lnkTabTransactions.CssClass = "portal-tab";
            lnkTabPointsHistory.CssClass = "portal-tab";
            lnkTabRedeem.CssClass = "portal-tab";

            // Hide all panels
            PanelTransactions.Visible = false;
            PanelPointsHistory.Visible = false;
            PanelRedeem.Visible = false;

            if (command == "Transactions")
            {
                lnkTabTransactions.CssClass = "portal-tab active";
                PanelTransactions.Visible = true;
            }
            else if (command == "PointsHistory")
            {
                lnkTabPointsHistory.CssClass = "portal-tab active";
                PanelPointsHistory.Visible = true;
            }
            else if (command == "Redeem")
            {
                lnkTabRedeem.CssClass = "portal-tab active";
                PanelRedeem.Visible = true;
            }
        }

        protected void RepeaterPricingRates_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "PrepareRedeem")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                if (args.Length == 2)
                {
                    if (int.TryParse(args[0], out int minutesToRedeem) && double.TryParse(args[1], out double pointsRequired))
                    {
                        string userId = HiddenField_UserId.Value;
                        double currentPoints = pointsRepo.GetPointsByUserId(userId);

                        if (currentPoints >= pointsRequired)
                        {
                            HF_RedeemMinutes.Value = minutesToRedeem.ToString();
                            HF_RedeemPoints.Value = pointsRequired.ToString();

                            litModalMinutes.Text = minutesToRedeem.ToString();
                            litModalPoints.Text = pointsRequired.ToString();
                            PanelRedeemModal.Visible = true;
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('Insufficient points to redeem this reward.');", true);
                        }
                    }
                }
            }
        }

        protected void btnConfirmRedeem_Click(object sender, EventArgs e)
        {
            if (int.TryParse(HF_RedeemMinutes.Value, out int minutesToRedeem) && double.TryParse(HF_RedeemPoints.Value, out double pointsRequired))
            {
                string userId = HiddenField_UserId.Value;
                double currentPoints = pointsRepo.GetPointsByUserId(userId);

                if (currentPoints >= pointsRequired)
                {
                    // 1. Deduct points via points repository
                    pointsRepo.Create(userId, "-" + pointsRequired, $"REDEEMED {minutesToRedeem} MINUTES");

                    // 2. Add time to user balance via balances repository
                    balanceRepo.Create(userId, minutesToRedeem.ToString(), "0.00", $"REDEEMED POINTS FOR {minutesToRedeem} MINS");

                    // Refresh page
                    Response.Redirect(Request.RawUrl);
                }
            }
        }

        protected void btnCancelRedeem_Click(object sender, EventArgs e)
        {
            PanelRedeemModal.Visible = false;
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

        protected void GridViewPoints_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewPoints.PageIndex = e.NewPageIndex;
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            userSession.RemoveCookies();
            Response.Redirect("~/Login.aspx?logout=true");
        }
    }
}