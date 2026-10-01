using BNet.Cafe.Server.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace BNet.Cafe.Server
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            new Databases.Tables.computers().create();
            new Databases.Tables.rentals().create();
            new Databases.Tables.users().create();
            new Databases.Tables.balances().create();
            new Databases.Tables.pricing_rates().create();
            new Databases.Tables.inventory_items().create();
            new Databases.Tables.inventory_transactions().create();
            new Databases.Tables.suppliers().create();

            DBContext DBContext = new DBContext();
            //DB UPDATE 
            StringBuilder queries = new StringBuilder();
            queries.AppendLine("UPDATE users SET DBRole = 'MEMBER' WHERE DBRole = 'USER';");
            queries.AppendLine("UPDATE pricing_rates SET DBCustomerType = 'MEMBER' WHERE DBCustomerType = 'USER';");
            DBContext.SqlExecuteAsync(queries.ToString());

            //Page.RegisterAsyncTask(new PageAsyncTask(async () =>
            //{

            //}));
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }
    }
}