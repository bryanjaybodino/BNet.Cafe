using Newtonsoft.Json;
using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Services
{
    public static class RemoteMessagingService
    {
        /// <summary>
        /// Formats payload and invokes the JS sendTextMessageToPC(targetClient, messageContent) function.
        /// </summary>
        /// <param name="control">The UserControl or Page instance calling the method.</param>
        /// <param name="targetClient">Target computer identifier (e.g., 'PC-01').</param>
        /// <param name="customerName">Name of the customer assigned to the session.</param>
        /// <param name="duration">Rental duration in minutes.</param>
        /// <param name="amount">Total rental amount charged.</param>
        /// <param name="command">Command directive (defaults to 'CREATE').</param>
        public static void SendToPC(Control control, string targetClient, string customerName, int duration, decimal amount, string command = "CREATE")
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control), "Control parameter cannot be null.");
            }

            // 1. Build message object
            var dataPayload = new
            {
                customerName = !string.IsNullOrEmpty(customerName) ? customerName : null,
                duration = duration,
                amount = amount,
                command = command,
                dateTime = TimeService.Get().ToString()
            };

            // 2. Convert data payload to JSON string
            string jsonMessage = JsonConvert.SerializeObject(dataPayload);

            // 3. Serialize both parameters so they format properly for JS execution
            string safeTargetClient = JsonConvert.SerializeObject(targetClient ?? string.Empty);
            string safeJsonMessage = JsonConvert.SerializeObject(jsonMessage);

            // 4. Construct JS call matching sendTextMessageToPC(targetClient, messageContent)
            string script = $"sendTextMessageToPC({safeTargetClient}, {safeJsonMessage});";

            // 5. Register startup script context for ASP.NET WebForms / UpdatePanel
            ScriptManager.RegisterStartupScript(control, control.GetType(), "SendToPCScript" + Guid.NewGuid().ToString(), script, true);
        }


        public static void TransferSession(Control control, string targetClient, string customerName, int duration, decimal amount, DateTime dateTime)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control), "Control parameter cannot be null.");
            }

            // 1. Build message object
            var dataPayload = new
            {
                customerName = !string.IsNullOrEmpty(customerName) ? customerName : null,
                duration = duration,
                amount = amount,
                command = ConstantData.RentalCommand.CREATE,
                dateTime = dateTime.ToString()
            };

            // 2. Convert data payload to JSON string
            string jsonMessage = JsonConvert.SerializeObject(dataPayload);

            // 3. Serialize both parameters so they format properly for JS execution
            string safeTargetClient = JsonConvert.SerializeObject(targetClient ?? string.Empty);
            string safeJsonMessage = JsonConvert.SerializeObject(jsonMessage);

            // 4. Construct JS call matching sendTextMessageToPC(targetClient, messageContent)
            string script = $"sendTextMessageToPC({safeTargetClient}, {safeJsonMessage});";

            // 5. Register startup script context for ASP.NET WebForms / UpdatePanel
            ScriptManager.RegisterStartupScript(control, control.GetType(), "SendToPCScript" + Guid.NewGuid().ToString(), script, true);
        }
        public static void LogoutPC(Control control, string targetClient)
        {
            SendTextMessage(control, targetClient, ConstantData.RentalCommand.LOGOUT);
        }

        public static void ShutdownPC(Control control, string targetClient)
        {
            SendTextMessage(control, targetClient, ConstantData.RentalCommand.SHUTDOWN);
        }
        public static void PausePC(Control control, string targetClient)
        {
            SendTextMessage(control, targetClient, ConstantData.RentalCommand.PAUSE);
        }
        public static void ResumePC(Control control, string targetClient)
        {
            SendTextMessage(control, targetClient, ConstantData.RentalCommand.RESUME);
        }
        public static void RestartPC(Control control, string targetClient)
        {
            SendTextMessage(control, targetClient, ConstantData.RentalCommand.RESTART);
        }
        public static void SendTextMessage(Control control, string targetClient, string message)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control), "Control parameter cannot be null."); 
            }

            // 1. Serialize parameters safely for JavaScript string literal execution
            string safeTargetClient = JsonConvert.SerializeObject(targetClient ?? string.Empty); 
            string safeMessage = JsonConvert.SerializeObject(message ?? string.Empty); 

            // 2. Construct client-side script call
            string script = $"sendTextMessageToPC({safeTargetClient}, {safeMessage});"; 

            // 3. Register script context for ASP.NET WebForms UpdatePanel compatibility
            ScriptManager.RegisterStartupScript(control, control.GetType(), "SendTextMessageScript" + Guid.NewGuid().ToString(), script, true); 
        }

    }
}