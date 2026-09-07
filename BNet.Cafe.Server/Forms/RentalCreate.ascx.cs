using BNet.Cafe.Server.Services;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Forms
{
    public partial class RentalCreate : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDropdowns();
            }
        }

        private void LoadDropdowns()
        {
            // Populate Computer DropDownList
            Repositories.Computers computers = new Repositories.Computers();
            DropDownList_Computer.DataSource = computers.GetAll();
            DropDownList_Computer.DataTextField = "DBComputerName";
            DropDownList_Computer.DataValueField = "DBId";
            DropDownList_Computer.DataBind();
            DropDownList_Computer.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Select Computer --", ""));




            //// Populate Customer DropDownList
            //Repositories.Customers customers = new Repositories.Customers();
            //DropDownList_Customer.DataSource = customers.GetAll();
            //DropDownList_Customer.DataTextField = "CustomerName";
            //DropDownList_Customer.DataValueField = "Id";
            //DropDownList_Customer.DataBind();
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            string computerId = DropDownList_Computer.SelectedValue;
            string customerId = DropDownList_Customer.SelectedValue;
            string duration = TextBox_Duration.Text.Trim();
            string amount = TextBox_Amount.Text.Trim();

            Repositories.Rentals rentals = new Repositories.Rentals();
            bool isSuccess = rentals.Create(computerId, customerId, duration, amount);

            if (isSuccess)
            {
                AlertService.ShowAlert(UpdatePanel1, "Rental session successfully created.", "success", "BNetPage.aspx?Form=Rentals");
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to create rental session.", "error");
            }
        }
    }
}