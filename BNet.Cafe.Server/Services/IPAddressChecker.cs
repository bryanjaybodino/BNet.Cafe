using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Web;

namespace BNet.Cafe.Server.Services
{
    public class IPAddressChecker
    {
        public bool IsHttps()
        {
            var context = HttpContext.Current;
            var request = context.Request;

            // ✅ 1. Check if direct connection is HTTPS
            if (request.IsSecureConnection)
                return true;

            // ✅ 2. Cloudflare Tunnel / reverse proxy sets this header
            string proto = request.Headers["X-Forwarded-Proto"];
            if (!string.IsNullOrEmpty(proto))
                return proto.Equals("https", StringComparison.OrdinalIgnoreCase);

            // ✅ 3. Some proxies use this instead
            string forwardedProto = request.Headers["CF-Visitor"];
            if (!string.IsNullOrEmpty(forwardedProto))
                return forwardedProto.Contains("\"scheme\":\"https\"");

            // ✅ 4. Azure / AWS load balancers sometimes use this
            string frontEndHttps = request.Headers["Front-End-Https"];
            if (!string.IsNullOrEmpty(frontEndHttps))
                return frontEndHttps.Equals("on", StringComparison.OrdinalIgnoreCase);

            return false;
        }
        private static readonly MemoryCache Cache = MemoryCache.Default;

        public string GetClientIP()
        {
            var context = HttpContext.Current;
            var headers = context.Request.Headers;

            // ✅ 1. Try CF-Connecting-IP first (works if tunnel config is correct)
            string ip = headers["CF-Connecting-IP"];

            // ✅ 2. Cloudflare Tunnel often uses this instead
            if (string.IsNullOrEmpty(ip))
                ip = headers["X-Real-IP"];

            // ✅ 3. X-Forwarded-For — take the FIRST IP (real visitor)
            if (string.IsNullOrEmpty(ip))
            {
                var forwarded = headers["X-Forwarded-For"];
                if (!string.IsNullOrEmpty(forwarded))
                    ip = forwarded.Split(',')[0].Trim();
            }

            // ✅ 4. Last resort — direct connection IP
            if (string.IsNullOrEmpty(ip))
                ip = context.Request.UserHostAddress;

            // ✅ 5. Validate
            if (!IPAddress.TryParse(ip, out var clientIp))
                return null;

            // ✅ 6. Cache per IP
            string cacheKey = "ClientIP_" + clientIp.ToString();
            if (Cache.Get(cacheKey) is string cachedResult)
                return cachedResult;

            // ✅ 7. Resolve
            string result = IPAddress.IsLoopback(clientIp) ? LocalIp : ResolveSubnet(clientIp);

            Cache.Set(cacheKey, result, DateTimeOffset.UtcNow.AddMinutes(10));
            return result;
        }

        private static readonly string LocalIp = GetLocalIpOnce();
        private static readonly NetworkInterface[] ActiveNics = GetActiveNicsOnce();
        private static string GetLocalIpOnce()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;

                foreach (var unicast in nic.GetIPProperties().UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                        return unicast.Address.ToString();
                }
            }
            return null;
        }
        private static NetworkInterface[] GetActiveNicsOnce()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .ToArray();
        }
        private string ResolveSubnet(IPAddress clientIp)
        {
            foreach (var nic in ActiveNics)
            {
                foreach (var unicast in nic.GetIPProperties().UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    if (IsOnSameSubnet(clientIp, unicast.Address, unicast.IPv4Mask))
                        return clientIp.ToString();
                }
            }

            return clientIp.ToString();
        }
        private bool IsOnSameSubnet(IPAddress clientIp, IPAddress serverIp, IPAddress subnetMask)
        {
            var clientBytes = clientIp.GetAddressBytes();
            var serverBytes = serverIp.GetAddressBytes();
            var maskBytes = subnetMask.GetAddressBytes();

            for (int i = 0; i < 4; i++)
            {
                if ((clientBytes[i] & maskBytes[i]) != (serverBytes[i] & maskBytes[i]))
                    return false;
            }
            return true;
        }

        public class BanUser
        {
            // ================================
            // 🔹 Ban System
            // ================================
            public bool IsBanned(string ip)
            {
                return HttpContext.Current.Cache["ban_" + ip] != null;
            }

            public void BanIP(string ip, int minutes)
            {
                DateTime expiry = DateTime.UtcNow.AddMinutes(minutes);

                HttpContext.Current.Cache.Insert(
                    "ban_" + ip,
                    expiry,
                    null,
                    expiry,
                    System.Web.Caching.Cache.NoSlidingExpiration
                );

            }
            public int GetRemainingBanMinutes(string ip)
            {
                var expiry = HttpContext.Current.Cache["ban_" + ip] as DateTime?;

                if (expiry == null)
                    return 0;

                var remaining = expiry.Value - DateTime.UtcNow;

                return remaining.TotalMinutes > 0
                    ? (int)Math.Ceiling(remaining.TotalSeconds)
                    : 0;
            }

        }



        public class LockUser
        {
            public bool IsLocked(string key)
            {
                return HttpContext.Current.Cache["lock_" + key] != null;
            }


            public int GetRemainingLockedMinutes(string key)
            {
                var expiry = HttpContext.Current.Cache["lock_" + key] as DateTime?;

                if (expiry == null)
                    return 0;

                var remaining = expiry.Value - DateTime.UtcNow;

                return remaining.TotalMinutes > 0
                    ? (int)Math.Ceiling(remaining.TotalSeconds)
                    : 0;
            }

            public void Lock(string key, int minutes)
            {
                DateTime expiry = DateTime.UtcNow.AddMinutes(minutes);

                HttpContext.Current.Cache.Insert(
                    "lock_" + key,
                    expiry,
                    null,
                    expiry,
                    System.Web.Caching.Cache.NoSlidingExpiration
                );
            }

            public int IncrementAttempt(string key, int seconds = 60)
            {
                string cacheKey = "attempt_" + key;

                var entry = HttpContext.Current.Cache[cacheKey] as Tuple<int, DateTime>;

                if (entry == null || entry.Item2 < DateTime.UtcNow)
                {
                    entry = new Tuple<int, DateTime>(1, DateTime.UtcNow.AddSeconds(seconds));
                }
                else
                {
                    entry = new Tuple<int, DateTime>(entry.Item1 + 1, entry.Item2);
                }

                HttpContext.Current.Cache.Insert(cacheKey, entry, null, entry.Item2, System.Web.Caching.Cache.NoSlidingExpiration);

                return entry.Item1;
            }
        }
    }
}