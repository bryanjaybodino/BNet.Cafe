using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms
{
    public partial class POS : System.Web.UI.UserControl
    {
        private readonly InventoryItems itemsRepo = new InventoryItems();
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();
        Sessions.User user = new Sessions.User();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitCart();
                LoadCategories();
                LoadCatalog();
                BindCart();
            }
        }

        private void InitCart()
        {
            if (ViewState["Cart"] == null)
            {
                DataTable cart = new DataTable();
                cart.Columns.Add("ItemId", typeof(string));
                cart.Columns.Add("ItemName", typeof(string));
                cart.Columns.Add("UnitPrice", typeof(double));
                cart.Columns.Add("Quantity", typeof(int));
                cart.Columns.Add("Subtotal", typeof(double));
                ViewState["Cart"] = cart;
            }
        }

        private void LoadCategories()
        {
            DataTable dt = itemsRepo.GetAll();
            DropDownList_Category.Items.Clear();
            DropDownList_Category.Items.Add(new ListItem("All Categories", ""));

            if (dt != null && dt.Rows.Count > 0)
            {
                DataView view = new DataView(dt);
                DataTable distinctCategories = view.ToTable(true, "DBCategory");

                foreach (DataRow row in distinctCategories.Rows)
                {
                    string category = row["DBCategory"].ToString().Trim();
                    if (!string.IsNullOrEmpty(category))
                    {
                        DropDownList_Category.Items.Add(new ListItem(category, category));
                    }
                }
            }
        }

        private void LoadCatalog()
        {
            string searchKeyword = TextBox_Search.Text.Trim();
            string selectedCategory = DropDownList_Category.SelectedValue;

            // Pass string.Empty or only search when not blank so GetAll SQL template builds correctly
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
                if (Panel_NoResults != null) Panel_NoResults.Visible = false;
            }
            else
            {
                Repeater_Products.DataSource = null;
                Repeater_Products.DataBind();
                Repeater_Products.Visible = false;
                if (Panel_NoResults != null) Panel_NoResults.Visible = true;
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            LoadCatalog();
        }

        public string GetProductImage(object itemIdObj)
        {
            if (itemIdObj == null) return "Uploads/default.png";
            string itemId = itemIdObj.ToString();
            string relativePath = $"~/Uploads/Inventory/{itemId}.png";
            string physicalPath = HttpContext.Current.Server.MapPath(relativePath);

            return File.Exists(physicalPath) ? $"Uploads/Inventory/{itemId}.png" : "Uploads/default.png";
        }

        protected void Repeater_Products_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "AddToCart")
            {
                string itemId = e.CommandArgument.ToString();
                DataTable itemData = itemsRepo.GetById(itemId);

                if (itemData != null && itemData.Rows.Count > 0)
                {
                    DataRow product = itemData.Rows[0];
                    int stock = Convert.ToInt32(product["DBQuantityInStock"]);
                    double price = Convert.ToDouble(product["DBUnitPrice"]);
                    string name = product["DBItemName"].ToString();

                    DataTable cart = (DataTable)ViewState["Cart"];
                    DataRow existingRow = null;

                    foreach (DataRow row in cart.Rows)
                    {
                        if (row["ItemId"].ToString() == itemId)
                        {
                            existingRow = row;
                            break;
                        }
                    }

                    if (existingRow != null)
                    {
                        int currentQty = Convert.ToInt32(existingRow["Quantity"]);
                        if (currentQty + 1 > stock)
                        {
                            AlertService.ShowAlert(UpdatePanel1, $"Cannot add more. Stock limit reached ({stock}).", "warning");
                            return;
                        }
                        existingRow["Quantity"] = currentQty + 1;
                        existingRow["Subtotal"] = (currentQty + 1) * price;
                    }
                    else
                    {
                        if (stock < 1)
                        {
                            AlertService.ShowAlert(UpdatePanel1, "Item is out of stock.", "warning");
                            return;
                        }
                        cart.Rows.Add(itemId, name, price, 1, price);
                    }

                    ViewState["Cart"] = cart;
                    BindCart();
                }
            }
        }

        protected void GridView_Cart_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            DataTable cart = (DataTable)ViewState["Cart"];
            string itemId = e.CommandArgument.ToString();

            DataRow targetRow = null;
            foreach (DataRow row in cart.Rows)
            {
                if (row["ItemId"].ToString() == itemId)
                {
                    targetRow = row;
                    break;
                }
            }

            if (targetRow != null)
            {
                if (e.CommandName == "IncreaseQty")
                {
                    DataTable itemData = itemsRepo.GetById(itemId);
                    int stock = Convert.ToInt32(itemData.Rows[0]["DBQuantityInStock"]);
                    int currentQty = Convert.ToInt32(targetRow["Quantity"]);

                    if (currentQty + 1 <= stock)
                    {
                        targetRow["Quantity"] = currentQty + 1;
                        targetRow["Subtotal"] = (currentQty + 1) * Convert.ToDouble(targetRow["UnitPrice"]);
                    }
                    else
                    {
                        AlertService.ShowAlert(UpdatePanel1, $"Max stock available is {stock}.", "warning");
                    }
                }
                else if (e.CommandName == "DecreaseQty")
                {
                    int currentQty = Convert.ToInt32(targetRow["Quantity"]);
                    if (currentQty > 1)
                    {
                        targetRow["Quantity"] = currentQty - 1;
                        targetRow["Subtotal"] = (currentQty - 1) * Convert.ToDouble(targetRow["UnitPrice"]);
                    }
                    else
                    {
                        cart.Rows.Remove(targetRow);
                    }
                }
                else if (e.CommandName == "RemoveItem")
                {
                    cart.Rows.Remove(targetRow);
                }

                ViewState["Cart"] = cart;
                BindCart();
            }
        }

        private void BindCart()
        {
            DataTable cart = (DataTable)ViewState["Cart"];
            GridView_Cart.DataSource = cart;
            GridView_Cart.DataBind();

            double total = 0;
            foreach (DataRow row in cart.Rows)
            {
                total += Convert.ToDouble(row["Subtotal"]);
            }
            Label_Total.Text = total.ToString("N2");
        }

        protected void LinkButton_Clear_Click(object sender, EventArgs e)
        {
            ClearCart();
        }

        private void ClearCart()
        {
            DataTable cart = (DataTable)ViewState["Cart"];
            cart.Clear();
            ViewState["Cart"] = cart;
            BindCart();
        }

        protected void LinkButton_Submit_Click(object sender, EventArgs e)
        {
            DataTable cart = (DataTable)ViewState["Cart"];
            if (cart == null || cart.Rows.Count == 0)
            {
                AlertService.ShowAlert(UpdatePanel1, "Your cart is empty.", "warning");
                return;
            }

            double totalAmount = 0;
            foreach (DataRow row in cart.Rows)
            {
                totalAmount += Convert.ToDouble(row["Subtotal"]);
            }

            string userId = user.user_id;
            bool allSuccess = true;

            foreach (DataRow row in cart.Rows)
            {
                string itemId = row["ItemId"].ToString();
                string qty = row["Quantity"].ToString();
                string cost = row["UnitPrice"].ToString();

                bool ok = transactionsRepo.StockOut(itemId, userId, qty, cost);
                if (!ok) allSuccess = false;
            }

            if (allSuccess)
            {
                ClearCart();
                LoadCatalog();

                POSReceipt.Show(totalAmount);
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to complete some items in the sale.", "error");
            }
        }
    }
}