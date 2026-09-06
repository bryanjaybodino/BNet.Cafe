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

            string script = $@"
                Sys.Application.add_load(function() {{
                    ShowAlert('{message}', '{type}');
                    {redirectScript}
                }});";

            ScriptManager.RegisterStartupScript(
                control,
                control.GetType(),
                "AlertScript",
                script,
                true
            );
        }
    }
}