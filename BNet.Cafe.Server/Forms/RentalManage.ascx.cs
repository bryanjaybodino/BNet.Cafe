using BNet.Cafe.Server.ConstantData;
using BNet.Cafe.Server.Services;
using Newtonsoft.Json; // Ensure Newtonsoft.Json is referenced
using Newtonsoft.Json.Linq;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class RentalManage : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDropdowns();
                string id = Request.QueryString["id"];
                Repositories.Computers computers = new Repositories.Computers();
                var computer = computers.GetById(id);
                Repositories.ClientData clientData = new Repositories.ClientData();

                for (int i = 0; i < computer.Rows.Count; i++)
                {
                    string computerId = computer.Rows[i]["DBId"].ToString();
                    string clientName = computer.Rows[i]["DBComputerName"].ToString();
                    string status = clientData.FetchStatus(clientName);
                    TextBox_ComputerName.Text = clientName;
                    Label_Status.Text = status;
                    if (status == "Occupied" || status == "Paused")
                    {
                        Repositories.Rentals rentals = new Repositories.Rentals();
                        var rental = rentals.GetByComputerId(id);
                        for (int ii = 0; ii < rental.Rows.Count; ii++)
                        {
                            string rentalId = rental.Rows[ii]["DBId"].ToString();
                            string duration = rental.Rows[ii]["DBDuration"].ToString();
                            string userId = rental.Rows[ii]["DBUserId"].ToString();
                            string dbAmount = rental.Rows[ii]["DBAmount"].ToString();

                            TextBox_Duration.Text = duration;
                            Label_RentalId.Text = rentalId;
                            Label_HeaderText.Text = "Rental ID # : ";

                            // Save original loaded values
                            HiddenField_InitialDuration.Value = duration;
                            HiddenField_InitialAmount.Value = string.IsNullOrEmpty(dbAmount) ? "0.00" : dbAmount;
                            DropDownList_Customer.SelectedValue = userId;

                            string userType = UserType.Guest;
                            if (int.TryParse(userId, out _))
                            {
                                Repositories.Users customers = new Repositories.Users();
                                var userData = customers.GetById(userId);
                                if (userData.Rows.Count > 0)
                                {
                                    userType = userData.Rows[0]["DBRole"].ToString();
                                }
                            }
                            HiddenField_Role.Value = userType;
                            var pricingRates = new Repositories.PricingRates();
                            var data = pricingRates.GetAll(userType, 0);

                            //Bawal mag open time si member since hourly base na siya
                            if (userType != UserType.Guest)
                            {
                                LinkButton_OpenTime.Visible = false;
                            }

                            if (data.Rows.Count == 0)
                            {
                                DisableFormAndShowAlert("No active pricing configuration found on your pricing settings. Please contact support if this issue persists.");
                            }
                            void DisableFormAndShowAlert(string alertMessage)
                            {
                                Label_Status.CssClass = "bnet-badge-status gray";
                                Panel_Form.Enabled = false;
                                Panel_Buttons.Visible = false;
                                Label_HeaderText.Text = "Pricing Unavailable";
                                AlertService.ShowAlert(this, alertMessage, "danger");
                            }

                        }
                        Label_Status.CssClass = "bnet-badge-status red";
                    }
                    else if (status == "Offline")
                    {
                        Label_Status.CssClass = "bnet-badge-status gray";
                        AlertService.ShowAlert(this, "The selected computer is currently offline. Please ensure the computer is online before creating a rental session.", "warning");
                        Panel_Form.Enabled = false;
                        Panel_Buttons.Visible = false;
                        Label_HeaderText.Text = "Currently Down";
                    }
                    else if (status == "Administrator")
                    {
                        Label_Status.CssClass = "bnet-badge-status yellow";
                        AlertService.ShowAlert(this, "The selected computer is currently maintenance. Please ensure the computer is online before creating a rental session.", "warning");
                        Panel_Form.Enabled = false;
                        Panel_Buttons.Visible = false;
                        Label_HeaderText.Text = "Maintenance";
                    }
                    else
                    {
                        Label_HeaderText.Text = "Add New Rental";
                        Label_Status.CssClass = "bnet-badge-status green";
                    }
                }
            }
        }

        private void LoadDropdowns()
        {
            //// Populate Customer DropDownList
            Repositories.Users customers = new Repositories.Users();
            DropDownList_Customer.DataSource = customers.GetAll();
            DropDownList_Customer.DataTextField = "DBEmail";
            DropDownList_Customer.DataValueField = "DBId";
            DropDownList_Customer.DataBind();
            DropDownList_Customer.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select Customer --", ""));
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string computerId = Request.QueryString["id"];
            string computerName = TextBox_ComputerName.Text.Trim();
            string userId = DropDownList_Customer.SelectedValue;
            string rentalId = Label_RentalId.Text;
            string command = (Label_Status.Text == "Occupied") ? ConstantData.RentalCommand.UPDATE : ConstantData.RentalCommand.CREATE;

            // Extract and parse form controls safely
            int duration = int.TryParse(TextBox_Duration.Text.Trim(), out int d) ? d : 0;
            decimal amount = decimal.TryParse(TextBox_Amount.Text.Trim(), out decimal a) ? a : 0m;

            Repositories.Rentals rentals = new Repositories.Rentals();

            bool isSuccess = false;
            if (command == ConstantData.RentalCommand.UPDATE)
            {
                isSuccess = rentals.Update(rentalId, computerId, userId, duration.ToString().Replace(",", ""), amount.ToString().Replace(",", ""));
            }
            else
            {
                isSuccess = rentals.Create(computerId, userId, duration.ToString().Replace(",", ""), amount.ToString().Replace(",", ""));
            }

            if (isSuccess)
            {
                RemoteMessagingService.SendToPC(UpdatePanel1, computerName, userId, duration, amount, command);
                AlertService.ShowAlert(UpdatePanel1, "Rental session successfully created.", "success", "BNetPage.aspx?Form=Computers");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to create rental session.", "error");
            }
        }
    }
}