using BNet.Cafe.Server.Repositories;
using BNet.Cafe.Server.Services;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace BNet.Cafe.Server.Forms
{
    public partial class Inventory : System.Web.UI.UserControl
    {
        private readonly InventoryItems inventoryItems = new InventoryItems();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            string search = TextBox_Search.Text.Trim();
            DataTable data = inventoryItems.GetAll(search, GridViewTemplateService.GetPaginationIndex(GridViewTable));
            GridViewTemplateService.SetGridView(GridViewTable, data, Panel_Pagination);

            for (int i = 0; i < GridViewTable.Rows.Count; i++)
            {
                Label Label_DBQuantityInStock = (Label)GridViewTable.Rows[i].FindControl("Label_DBQuantityInStock");
                Label Label_DBReorderLevel = (Label)GridViewTable.Rows[i].FindControl("Label_DBReorderLevel");

                if (Label_DBQuantityInStock != null && Label_DBReorderLevel != null)
                {
                    int.TryParse(Label_DBQuantityInStock.Text, out int stock);
                    int.TryParse(Label_DBReorderLevel.Text, out int reorderLevel);

                    if (stock == 0)
                    {
                        Label_DBQuantityInStock.CssClass = "bnet-badge-status yellow";
                    }
                    else if (stock <= reorderLevel)
                    {
                        Label_DBQuantityInStock.CssClass = "bnet-badge-status blue";
                    }
                    else
                    {
                        Label_DBQuantityInStock.CssClass = "bnet-badge-status green";
                    }
                }
            }
        }

        protected void GridViewTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewTable.PageIndex = e.NewPageIndex;
        }

        protected void TextBox_Search_TextChanged(object sender, EventArgs e)
        {
            GridViewTable.PageIndex = 0;
        }
    }
}