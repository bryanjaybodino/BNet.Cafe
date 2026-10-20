using System;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BNet.Cafe.Server.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BNet.Cafe.Server.Services;

namespace BNet.Cafe.Server.Forms
{
    public partial class POS : System.Web.UI.UserControl
    {
        private readonly InventoryItems itemsRepo = new InventoryItems();
        private readonly InventoryTransactions transactionsRepo = new InventoryTransactions();
        private readonly Repositories.Users userRepo = new Repositories.Users();
        private readonly Repositories.Computers computersRepo = new Repositories.Computers();
        private readonly Repositories.Rentals rentalsRepo = new Repositories.Rentals();
        private readonly Repositories.Points pointsRepo = new Repositories.Points();
        Sessions.User user = new Sessions.User();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitCart();
                LoadCategories();
                LoadCatalog();
                LoadComputers();
                LoadCustomers();

                // AUTO-DETECT COMPUTER, USER, AND BIND CHAT ORDER ITEMS
                AutoDetectComputerAndUser();
                ProcessIncomingChatOrder();

                BindCart();
            }
        }

        private void LoadComputers()
        {
            DataTable dt = computersRepo.GetAll();
            DropDownList_POSComputer.DataSource = dt;
            DropDownList_POSComputer.DataTextField = "DBComputerName";
            DropDownList_POSComputer.DataValueField = "DBId";
            DropDownList_POSComputer.DataBind();
            DropDownList_POSComputer.Items.Insert(0, new ListItem("-- Select PC Terminal --", ""));
        }

        private void LoadCustomers()
        {
            DataTable dt = userRepo.GetAll();
            DropDownList_POSCustomer.DataSource = dt;
            DropDownList_POSCustomer.DataTextField = "DBEmail";
            DropDownList_POSCustomer.DataValueField = "DBId";
            DropDownList_POSCustomer.DataBind();
            DropDownList_POSCustomer.Items.Insert(0, new ListItem("-- Walk-in / Guest --", ""));
        }

        private void AutoDetectComputerAndUser()
        {
            string targetComputer = Request.QueryString["ComputerName"] ?? Request.QueryString["clientName"] ?? Request.QueryString["ClientName"];
            string targetUserId = Request.QueryString["UserId"] ?? Request.QueryString["userId"];

            if (!string.IsNullOrEmpty(targetComputer))
            {
                ListItem pcItem = DropDownList_POSComputer.Items.FindByText(targetComputer.ToUpper());
                if (pcItem != null)
                {
                    DropDownList_POSComputer.SelectedValue = pcItem.Value;

                    if (string.IsNullOrEmpty(targetUserId))
                    {
                        DataTable activeRental = rentalsRepo.GetByComputerId(pcItem.Value);
                        if (activeRental != null && activeRental.Rows.Count > 0)
                        {
                            targetUserId = activeRental.Rows[0]["DBUserId"].ToString();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(targetUserId))
            {
                ListItem userItem = DropDownList_POSCustomer.Items.FindByValue(targetUserId);
                if (userItem != null)
                {
                    DropDownList_POSCustomer.SelectedValue = userItem.Value;
                }
            }

            UpdatePointsDisplay();
        }

        private void ProcessIncomingChatOrder()
        {
            string orderItemsJson = Request.QueryString["OrderItems"];
            if (string.IsNullOrEmpty(orderItemsJson)) return;

            try
            {
                string decodedJson = Server.UrlDecode(orderItemsJson);
                JArray orderItems = JArray.Parse(decodedJson);
                DataTable cart = (DataTable)ViewState["Cart"];

                foreach (var item in orderItems)
                {
                    string itemName = item["name"]?.ToString()?.Trim();
                    int qty = item["qty"]?.Value<int>() ?? 1;

                    if (string.IsNullOrEmpty(itemName)) continue;

                    // Search inventory item by name match
                    DataTable allItems = itemsRepo.GetAll();
                    DataRow matchedProduct = null;

                    foreach (DataRow row in allItems.Rows)
                    {
                        if (row["DBItemName"].ToString().Trim().Equals(itemName, StringComparison.OrdinalIgnoreCase))
                        {
                            matchedProduct = row;
                            break;
                        }
                    }

                    if (matchedProduct != null)
                    {
                        string itemId = matchedProduct["DBId"].ToString();
                        double price = Convert.ToDouble(matchedProduct["DBUnitPrice"]);
                        int stock = Convert.ToInt32(matchedProduct["DBQuantityInStock"]);

                        int finalQty = Math.Min(qty, stock > 0 ? stock : qty);
                        if (finalQty <= 0) continue;

                        DataRow existingRow = null;
                        foreach (DataRow cartRow in cart.Rows)
                        {
                            if (cartRow["ItemId"].ToString() == itemId)
                            {
                                existingRow = cartRow;
                                break;
                            }
                        }

                        if (existingRow != null)
                        {
                            existingRow["Quantity"] = finalQty;
                            existingRow["Subtotal"] = finalQty * price;
                        }
                        else
                        {
                            cart.Rows.Add(itemId, itemName, price, finalQty, finalQty * price);
                        }
                    }
                }

                ViewState["Cart"] = cart;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error parsing chat order query string: " + ex.Message);
            }
        }

        protected void DropDownList_POSComputer_SelectedIndexChanged(object sender, EventArgs e)
        {
            string computerId = DropDownList_POSComputer.SelectedValue;
            if (!string.IsNullOrEmpty(computerId))
            {
                DataTable activeRental = rentalsRepo.GetByComputerId(computerId);
                if (activeRental != null && activeRental.Rows.Count > 0)
                {
                    string userId = activeRental.Rows[0]["DBUserId"].ToString();
                    ListItem userItem = DropDownList_POSCustomer.Items.FindByValue(userId);
                    DropDownList_POSCustomer.SelectedValue = userItem != null ? userItem.Value : "";
                }
                else
                {
                    DropDownList_POSCustomer.SelectedValue = "";
                }
            }

            UpdatePointsDisplay();
            BindCart();
        }

        protected void DropDownList_POSCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePointsDisplay();
            BindCart();
        }

        protected void CheckBox_UsePoints_CheckedChanged(object sender, EventArgs e)
        {
            BindCart();
        }

        private void UpdatePointsDisplay()
        {
            string customerId = DropDownList_POSCustomer.SelectedValue;
            if (!string.IsNullOrEmpty(customerId))
            {
                double points = pointsRepo.GetPointsByUserId(customerId);
                litAvailablePoints.Text = points.ToString("N2");
                Panel_Points.Visible = true;
            }
            else
            {
                Panel_Points.Visible = false;
                CheckBox_UsePoints.Checked = false;
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

            if (CheckBox_UsePoints.Checked && !string.IsNullOrEmpty(DropDownList_POSCustomer.SelectedValue))
            {
                string customerId = DropDownList_POSCustomer.SelectedValue;
                double availablePoints = pointsRepo.GetPointsByUserId(customerId);

                double pointsToApply = Math.Min(availablePoints, total);
                total -= pointsToApply;
            }

            Label_Total.Text = total < 0 ? "0.00" : total.ToString("N2");
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
            CheckBox_UsePoints.Checked = false;
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

            double subtotalAmount = 0;
            foreach (DataRow row in cart.Rows)
            {
                subtotalAmount += Convert.ToDouble(row["Subtotal"]);
            }

            string customerId = DropDownList_POSCustomer.SelectedValue;
            double pointsDeducted = 0;

            if (CheckBox_UsePoints.Checked && !string.IsNullOrEmpty(customerId))
            {
                double availablePoints = pointsRepo.GetPointsByUserId(customerId);
                pointsDeducted = Math.Min(availablePoints, subtotalAmount);
            }

            double finalTotalPayable = subtotalAmount - pointsDeducted;

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
                if (pointsDeducted > 0 && !string.IsNullOrEmpty(customerId))
                {
                    pointsRepo.Create(customerId, (-pointsDeducted).ToString(), "Points Redeemed for POS Purchase");
                }

                ClearCart();
                LoadCatalog();
                UpdatePointsDisplay();

                POSReceipt.Show(finalTotalPayable);
            }
            else
            {
                AlertService.ShowAlert(UpdatePanel1, "Failed to complete some items in the sale.", "error");
            }
        }
    }
}