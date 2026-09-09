using BNet.Cafe.Server.Services;
using Newtonsoft.Json; // Ensure Newtonsoft.Json is referenced
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
                    if (status == "Occupied")
                    {
                        Repositories.Rentals rentals = new Repositories.Rentals();
                        var rental = rentals.GetByComputerId(id);
                        for (int ii = 0; ii < rental.Rows.Count; ii++)
                        {
                            string rentalId = rental.Rows[ii]["DBId"].ToString();
                            string duration = rental.Rows[ii]["DBDuration"].ToString();
                            string customerId = rental.Rows[ii]["DBCustomerId"].ToString();
                            TextBox_Duration.Text = duration;
                            Label_RentalId.Text = rentalId;
                        }


                        Label_Status.CssClass = "badge-status red";
                    }
                    else if (status == "Offline")
                    {
                        Label_Status.CssClass = "badge-status gray";
                        AlertService.ShowAlert(UpdatePanel1, "The selected computer is currently offline. Please ensure the computer is online before creating a rental session.", "warning");
                        Panel_Form.Enabled = false;
                    }
                    else
                    {
                        Label_Status.CssClass = "badge-status green";
                    }
                }
                LoadDropdowns();
            }
        }

        private void LoadDropdowns()
        {
            //// Populate Customer DropDownList
            //Repositories.Customers customers = new Repositories.Customers();
            //DropDownList_Customer.DataSource = customers.GetAll();
            //DropDownList_Customer.DataTextField = "DBCustomerName";
            //DropDownList_Customer.DataValueField = "DBId";
            //DropDownList_Customer.DataBind();
            //DropDownList_Customer.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select Customer --", ""));
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string computerId = Request.QueryString["id"];
            string computerName = TextBox_ComputerName.Text.Trim();
            string customerId = DropDownList_Customer.SelectedValue;
            string rentalId = Label_RentalId.Text;
            string command = (Label_Status.Text == "Occupied") ? ConstantData.RentalCommand.UPDATE : ConstantData.RentalCommand.CREATE;

            // Extract and parse form controls safely
            int duration = int.TryParse(TextBox_Duration.Text.Trim(), out int d) ? d : 0;
            decimal amount = decimal.TryParse(TextBox_Amount.Text.Trim(), out decimal a) ? a : 0m;

            Repositories.Rentals rentals = new Repositories.Rentals();

            bool isSuccess = false;
            if (command == ConstantData.RentalCommand.UPDATE)
            {
                isSuccess = rentals.Update(rentalId, computerId, customerId, duration.ToString().Replace(",", ""), amount.ToString().Replace(",", ""));
            }
            else
            {
                isSuccess = rentals.Create(computerId, customerId, duration.ToString().Replace(",", ""), amount.ToString().Replace(",", ""));
            }

            if (isSuccess)
            {
                RemoteMessagingService.SendToPC(UpdatePanel1, computerName, customerId, duration, amount, command);
                AlertService.ShowAlert(UpdatePanel1, "Rental session successfully created.", "success", "BNetPage.aspx?Form=Computers");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to create rental session.", "error");
            }
        }
    }
}