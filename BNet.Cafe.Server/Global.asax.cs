using BNet.Cafe.Server.BNetWebsocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace BNet.Cafe.Server
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Setup setup = new Setup();
            setup.StartWebsocket();


            new Databases.Tables.computers().create();

            //Page.RegisterAsyncTask(new PageAsyncTask(async () =>
            //{

            //}));
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }
    }
}