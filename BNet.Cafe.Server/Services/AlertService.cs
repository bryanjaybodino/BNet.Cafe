using System;
using System.Web.UI;

namespace BNet.Cafe.Server.Services
{
    public static class AlertService
    {
        public static void ShowAlert(Control control, string message, string type, string redirectUrl = null, int delayMs = 1500)
        {
            // Sanitize string for safety
            string safeMessage = message?.Replace(@"\", @"\\").Replace("'", @"\'").Replace("\r\n", " ").Replace("\n", " ");

            string redirectScript = string.IsNullOrEmpty(redirectUrl)
                ? string.Empty
                : $"setTimeout(function(){{ if(typeof navigateTo === 'function') navigateTo('{redirectUrl}'); else window.location.href = '{redirectUrl}'; }}, {delayMs});";

            // Wait for DOM & JavaScript assets to load before calling custom functions
            string script = $@"
                (function() {{
                    function triggerAlert() {{
                        if (typeof ShowAlert === 'function') {{
                            ShowAlert('{safeMessage}', '{type}');
                            {redirectScript}
                        }} else {{
                            console.error('ShowAlert function is not defined. Ensure your JS file is loaded.');
                        }}
                    }}

                    if (document.readyState === 'loading') {{
                        document.addEventListener('DOMContentLoaded', triggerAlert);
                    }} else {{
                        triggerAlert();
                    }}
                }})();";

            ScriptManager.RegisterStartupScript(
                control,
                control.GetType(),
                "AlertScript_" + Guid.NewGuid().ToString("N"),
                script,
                true
            );
        }
    }
}