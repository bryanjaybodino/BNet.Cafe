using System.Web.UI;

namespace BNet.Cafe.Server.Services
{
    public static class AlertService
    {
        public static void ShowAlert(Control control, string message, string type, string redirectUrl = null, int delayMs = 1500)
        {
            string redirectScript = string.IsNullOrEmpty(redirectUrl)
                ? string.Empty
                : $"setTimeout(function(){{ navigateTo('{redirectUrl}') }}, {delayMs});";

            // Use an IIFE instead of Sys.Application.add_load to prevent re-execution on future postbacks
            string script = $@"
                (function() {{
                    ShowAlert('{message.Replace("'", "\\'")}', '{type}');
                    {redirectScript}
                }})();";

            ScriptManager.RegisterStartupScript(
                control,
                control.GetType(),
                "AlertScript_" + System.Guid.NewGuid().ToString("N"), // Unique key to avoid script overwrites
                script,
                true
            );
        }
    }
}