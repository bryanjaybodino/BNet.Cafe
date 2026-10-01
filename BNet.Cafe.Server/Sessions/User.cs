using BNet.Cafe.Server.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.Sessions
{
    public class User
    {
        void SetCookie(string name, string value, bool encrypt = true)
        {
            DateTime expires = DateTime.Now.AddDays(30);
            HttpContext.Current.Response.Cookies.Remove(cookiePrefix + name);
            var cookie = new HttpCookie(cookiePrefix + name);
            cookie.Value = encrypt ? SecuredDataService.Encrypted(value ?? "") : value ?? "";
            cookie.Expires = expires;
            cookie.HttpOnly = true;
            cookie.Secure = HttpContext.Current.Request.IsSecureConnection;
            // SameSite=Strict blocks cookies on the first request when the OS
            // launches the PWA (treated as a top-level cross-site navigation),
            // making count==0 and forcing a redirect to Login on every reopen.
            cookie.SameSite = SameSiteMode.Lax;
            HttpContext.Current.Response.Cookies.Set(cookie);
        }

        private string cookiePrefix
        {
            get
            {
                return "bnet_";
            }
        }

        public string imap_hostname
        {
            get
            {
                return "mail.gmail.com";
            }
        }
        public int imap_port
        {
            get
            {
                return 993;
            }
        }
        public int count
        {
            get
            {
                if (HttpContext.Current.Request.Cookies[cookiePrefix + "user_email"] != null)
                {
                    return 1;
                }
                return 0;
            }
        }
        public string user_fullname
        {
            get
            {
                string value = "";
                if (HttpContext.Current.Request.Cookies[cookiePrefix + "user_fullname"] != null)
                {
                    value = SecuredDataService.Decrypted(HttpContext.Current.Request.Cookies[cookiePrefix + "user_fullname"].Value);
                }
                return value.ToLower();
            }
        }
        public string user_role
        {
            get
            {
                string value = "";
                if (HttpContext.Current.Request.Cookies[cookiePrefix + "user_role"] != null)
                {
                    value = SecuredDataService.Decrypted(HttpContext.Current.Request.Cookies[cookiePrefix + "user_role"].Value);
                }
                return value.ToLower();
            }
        }
        public bool isUser
        {
            get
            {
                return (user_role == "user");
            }
        }

        public string user_email
        {
            get
            {
                string value = "";
                if (HttpContext.Current.Request.Cookies[cookiePrefix + "user_email"] != null)
                {
                    value = SecuredDataService.Decrypted(HttpContext.Current.Request.Cookies[cookiePrefix + "user_email"].Value);
                }
                return value.ToLower();
            }
        }
        public string user_id
        {
            get
            {
                string value = "";
                if (HttpContext.Current.Request.Cookies[cookiePrefix + "user_id"] != null)
                {
                    value = SecuredDataService.Decrypted(HttpContext.Current.Request.Cookies[cookiePrefix + "user_id"].Value);
                }
                return value.ToLower();
            }
        }
        public void createCookies(string user_email, string user_fullname, string user_role,string user_id)
        {
            SetCookie("user_email", user_email);
            SetCookie("user_fullname", user_fullname);
            SetCookie("user_role", user_role);
            SetCookie("user_id", user_id);
        }
        public void RemoveCookies()
        {
            string[] cookies =
            {
                "user_email",
                "user_fullname",
                "user_role",
                "user_id",
            };

            foreach (string name in cookies)
            {
                // BUG FIX: Must match the original cookie's HttpOnly + SameSite attributes
                // when expiring, otherwise some browsers ignore the deletion instruction.
                var cookie = new HttpCookie(cookiePrefix + name)
                {
                    Expires = DateTime.Now.AddDays(-30),
                    Value = "",
                    HttpOnly = true,
                    SameSite = SameSiteMode.Lax
                };
                HttpContext.Current.Response.Cookies.Set(cookie);
            }

            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }

        public void SetBanCookies(string ip, DateTime? banExpiry)
        {
            if (!banExpiry.HasValue) return;

            var expiry = banExpiry.Value;

            var c1 = new HttpCookie(cookiePrefix + "ban_expiry", expiry.ToString("o"));
            c1.Expires = expiry;
            c1.HttpOnly = false;
            c1.SameSite = SameSiteMode.Lax;
            HttpContext.Current.Response.Cookies.Set(c1);

            var c2 = new HttpCookie(cookiePrefix + "ban_ip", ip);
            c2.Expires = expiry;
            c2.HttpOnly = false;
            c2.SameSite = SameSiteMode.Lax;
            HttpContext.Current.Response.Cookies.Set(c2);
        }
        public void RemoveBanCookies()
        {
            var expired = DateTime.UtcNow.AddDays(-1);

            var c1 = new HttpCookie(cookiePrefix + "ban_expiry", "")
            {
                Expires = expired,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            };
            HttpContext.Current.Response.Cookies.Set(c1);

            var c2 = new HttpCookie(cookiePrefix + "ban_ip", "")
            {
                Expires = expired,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            };
            HttpContext.Current.Response.Cookies.Set(c2);
        }
        public DateTime? ban_expiry
        {
            get
            {
                try
                {

                    var cookie = HttpContext.Current.Request.Cookies[cookiePrefix + "ban_expiry"];
                    return Convert.ToDateTime(cookie?.Value);
                }
                catch { return null; }
            }
        }


        public string ban_ip
        {
            get
            {
                var cookie = HttpContext.Current.Request.Cookies[cookiePrefix + "ban_ip"];
                return cookie?.Value ?? "";
            }
        }

    }
}