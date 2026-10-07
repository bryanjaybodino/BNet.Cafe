using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using BNet.Cafe.Server.Sessions;

namespace BNet.Cafe.Server
{
    public partial class Shop : Page
    {
        private readonly User userSession = new User();
        private readonly InventoryItems itemsRepo = new InventoryItems();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FileCssHelper.BundleCss();
                FileJsHelpler.BundleAddScripts(ScriptManager1, "Shop/Script");
                LoadCategories();
                LoadProducts();
            }
        }

        private void LoadCategories()
        {
            DataTable dt = itemsRepo.GetAll();
            DropDownList_Category.Items.Clear();
            DropDownList_Category.Items.Add(new System.Web.UI.WebControls.ListItem("All Categories", ""));

            if (dt != null && dt.Rows.Count > 0)
            {
                DataView view = new DataView(dt);
                DataTable distinctCategories = view.ToTable(true, "DBCategory");

                foreach (DataRow row in distinctCategories.Rows)
                {
                    string category = row["DBCategory"].ToString().Trim();
                    if (!string.IsNullOrEmpty(category))
                    {
                        DropDownList_Category.Items.Add(new System.Web.UI.WebControls.ListItem(category, category));
                    }
                }
            }
        }

        private void LoadProducts()
        {
            string searchKeyword = TextBox_Search.Text.Trim();
            string selectedCategory = DropDownList_Category.SelectedValue;

            DataTable dt = itemsRepo.GetAll(searchKeyword);

            if (dt != null && !string.IsNullOrEmpty(selectedCategory))
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = $"DBCategory = '{selectedCategory.Replace("'", "''")}'";
                dt = dv.ToTable();
            }

            if (dt != null && dt.Rows.Count > 0)
            {
                Repeater_Products.DataSource = dt;
                Repeater_Products.DataBind();
                Repeater_Products.Visible = true;
                Panel_NoResults.Visible = false;
            }
            else
            {
                Repeater_Products.DataSource = null;
                Repeater_Products.DataBind();
                Repeater_Products.Visible = false;
                Panel_NoResults.Visible = true;
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            LoadProducts();
        }

        protected void LinkButton_SaveMessage_Click(object sender, EventArgs e)
        {
            string message = TextBox_ChatMessage.Text;
            string computerName = TextBox_ComputerName.Text;
            string userId = userSession.count > 0
                ? userSession.user_id
                : ConstantData.UserType.Guest;

            if (!string.IsNullOrEmpty(message))
            {
                ChatMessages repo = new ChatMessages();
                repo.Create(message, computerName, userId);
            }

            TextBox_ChatMessage.Text = string.Empty;
        }

        public string GetProductImage(object itemIdObj)
        {
            if (itemIdObj == null) return "Uploads/default.png";
            string itemId = itemIdObj.ToString();
            string relativePath = $"~/Uploads/Inventory/{itemId}.png";
            string physicalPath = HttpContext.Current.Server.MapPath(relativePath);

            return File.Exists(physicalPath) ? $"Uploads/Inventory/{itemId}.png" : "Uploads/default.png";
        }
    }
}