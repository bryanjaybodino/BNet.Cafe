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
                Repositories.Computers computers = new Repositories.Computers();
                var computer = computers.GetById(Request.QueryString["id"]);
                for (int i = 0; i < computer.Rows.Count; i++)
                {
                    TextBox_ComputerName.Text = computer.Rows[i]["DBComputerName"].ToString();
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