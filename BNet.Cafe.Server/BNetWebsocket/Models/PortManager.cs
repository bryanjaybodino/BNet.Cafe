using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace BNet.Cafe.Server.BNetWebsocket
{
    public class PortManager
    {
        private static string RunNetsh(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
        }

        // Add URL ACL reservation (allows non-admin apps to bind to the URL)
        public static void AddUrlAcl(string url = "http://*:7891/")
        {
            string result = RunNetsh($"http add urlacl url={url} user=Everyone");
            Console.WriteLine($"AddUrlAcl: {result}");
        }

        // Remove URL ACL
        public static void RemoveUrlAcl(string url = "http://*:7891/")
        {
            string result = RunNetsh($"http delete urlacl url={url}");
            Console.WriteLine($"RemoveUrlAcl: {result}");
        }

        // Open port in Windows Firewall (skips if rule already exists)
        public static void OpenFirewallPort(int port = 7891, string ruleName = "MyHttpServer")
        {
            string check = RunNetsh($"advfirewall firewall show rule name=\"{ruleName}\"");

            if (check.Contains("No rules match the specified criteria"))
            {
                string result = RunNetsh(
                    $"advfirewall firewall add rule name=\"{ruleName}\" " +
                    $"dir=in action=allow protocol=TCP localport={port}"
                );
                Console.WriteLine($"OpenFirewall: Rule \"{ruleName}\" added. {result}");
            }
            else
            {
                Console.WriteLine($"OpenFirewall: Rule \"{ruleName}\" already exists, skipping.");
            }
        }

        // Remove firewall rule
        public static void CloseFirewallPort(string ruleName = "MyHttpServer")
        {
            string result = RunNetsh($"advfirewall firewall delete rule name=\"{ruleName}\"");
            Console.WriteLine($"CloseFirewall: {result}");
        }
    }
}