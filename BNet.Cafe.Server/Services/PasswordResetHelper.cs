using System;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public static class PasswordResetHelper
    {
        public static string GenerateResetUrl(string userId)
        {
            long expiryTicks = DateTime.UtcNow.AddMinutes(10).Ticks;
            string payload = $"{userId}|{expiryTicks}";

            // 1. Encrypt raw payload
            string rawEncrypted = SecuredDataService.Encrypted(payload);

            // 2. Make URL-safe Base64 token (removes %2f, %2b)
            string decoded = HttpUtility.UrlDecode(rawEncrypted);
            string urlSafeToken = HttpUtility.UrlEncode(decoded.Replace('+', '-').Replace('/', '_').TrimEnd('='));

            // 3. Retain the application folder path (/BNet.Cafe.Server/ResetPassword.aspx)
            var request = HttpContext.Current.Request;
            string baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}{request.ApplicationPath.TrimEnd('/')}";

            return $"{baseUrl}/ResetPassword.aspx?token={urlSafeToken}";
        }

        public static bool TryValidateToken(string token, out string userId, out bool isExpired)
        {
            userId = null;
            isExpired = false;

            if (string.IsNullOrEmpty(token)) return false;

            // Restore standard Base64 characters
            string base64Token = token.Replace('-', '+').Replace('_', '/');
            int padding = 4 - (base64Token.Length % 4);
            if (padding < 4)
            {
                base64Token += new string('=', padding);
            }

            string decrypted = SecuredDataService.Decrypted(base64Token);
            if (string.IsNullOrEmpty(decrypted)) return false;

            string[] parts = decrypted.Split('|');
            if (parts.Length != 2) return false;

            userId = parts[0];
            if (!long.TryParse(parts[1], out long expiryTicks)) return false;

            if (DateTime.UtcNow.Ticks > expiryTicks)
            {
                isExpired = true;
                return false;
            }

            return true;
        }
    }
}