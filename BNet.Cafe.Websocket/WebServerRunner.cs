using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace BNet.Cafe.Websocket
{
    internal class WebServerRunner
    {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        const int DefaultPort = 8180;
        const string DefaultPageName = "Default.aspx";

        static string defaultPage = DefaultPageName;
        static string virtualDir = "/";

        public static void Start(string[] args)
        {
            string taskName = "ASPHostingServer_AutoStart";

            // ---- 1. If the Scheduled Task already exists, skip the UAC prompt block entirely! ----
            if (!TaskExists(taskName))
            {
                // ---- 2. First-run only: Ensure we are running as Administrator to create the task ----
                if (!IsAdministrator())
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Assembly.GetExecutingAssembly().Location,
                            Arguments = string.Join(" ", args),
                            Verb = "runas", // Triggers UAC prompt ONLY ONCE ever
                            UseShellExecute = true
                        });
                    }
                    catch { }
                    return; // Exit non-admin instance
                }

                // ---- 3. Register the highest-privilege task on first run ----
                RegisterHighestPrivilegeTask(taskName);
            }

            // Global safety net: Catch unhandled exceptions on any thread and auto-restart
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                RestartApplication();
            };

            int port = DefaultPort;
            LoadConfig(ref port, ref defaultPage, ref virtualDir);

            if (args.Length > 0)
            {
                int argPort;
                if (int.TryParse(args[0], out argPort)) port = argPort;
            }
            if (args.Length > 1)
            {
                virtualDir = "/" + args[1].Trim().TrimStart('/', '\\');
            }

            if (virtualDir == "/" || string.IsNullOrWhiteSpace(virtualDir))
            {
                virtualDir = "/";
            }
            else
            {
                virtualDir = "/" + virtualDir.Trim().TrimStart('/', '\\').TrimEnd('/', '\\');
            }

            // Hide Console Window
            IntPtr handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, SW_HIDE);
            }

            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDir = Path.GetDirectoryName(exePath);

            string physicalDir;
            if (Path.GetFileName(exeDir).Equals("bin", StringComparison.OrdinalIgnoreCase))
                physicalDir = Directory.GetParent(exeDir).FullName + Path.DirectorySeparatorChar;
            else
                physicalDir = exeDir + Path.DirectorySeparatorChar;

            string binDir = Path.Combine(physicalDir, "bin");
            Directory.CreateDirectory(binDir);

            string targetExe = Path.Combine(binDir, Path.GetFileName(exePath));
            if (!string.Equals(Path.GetFullPath(exePath), Path.GetFullPath(targetExe), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Copy(exePath, targetExe, true);
                }
                catch { }
            }

            // ---- Configure Port and Firewall using PortManager (now running as Admin) ----
            string prefix = $"http://+:{port}/";
            try
            {
                PortManager.AddUrlAcl(prefix);
                PortManager.OpenFirewallPort(port, "ASPHosting_Port_" + port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Port config error: " + ex.Message);
            }

            WebHost host;
            try
            {
                host = (WebHost)ApplicationHost.CreateApplicationHost(
                    typeof(WebHost), virtualDir, physicalDir);
            }
            catch (Exception ex)
            {
                // If the app domain creation fails during a file update, trigger a restart
                File.AppendAllText("crash_log.txt", $"{DateTime.Now}: AppHost init failed: {ex}\n\n");
                RestartApplication();
                return;
            }

            var listener = new HttpListener();
            listener.Prefixes.Add(prefix);

            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Listener start error: " + ex.Message);
                RestartApplication();
                return;
            }

            while (true)
            {
                HttpListenerContext ctx;
                try
                {
                    ctx = listener.GetContext();
                }
                catch
                {
                    continue;
                }

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        Handle(ctx, host, physicalDir, virtualDir, port);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            File.AppendAllText("crash_log.txt", $"{DateTime.Now}: Request error: {ex}\n\n");

                            // Force an application restart if the AppDomain has been unloaded due to file replacements
                            if (ex is System.AppDomainUnloadedException || (ex.InnerException != null && ex.InnerException is System.AppDomainUnloadedException))
                            {
                                RestartApplication();
                                return;
                            }

                            ctx.Response.StatusCode = 500;
                            byte[] msg = Encoding.UTF8.GetBytes("Internal Server Error");
                            ctx.Response.OutputStream.Write(msg, 0, msg.Length);
                        }
                        catch { }
                    }
                    finally
                    {
                        try { ctx.Response.Close(); } catch { }
                    }
                });
            }
        }

        static void RestartApplication()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;

                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,          // Required to hide the window completely
                    CreateNoWindow = true,            // Prevents the console window from being created
                    WindowStyle = ProcessWindowStyle.Hidden // Ensures it stays hidden if a window handle is spawned
                });
            }
            catch { }
            finally
            {
                Environment.Exit(1);
            }
        }

        static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        static bool TaskExists(string taskName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = $"/query /tn \"{taskName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        static void RegisterHighestPrivilegeTask(string taskName)
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;

                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = $"/create /tn \"{taskName}\" /tr \"\\\"{exePath}\\\"\" /sc ONLOGON /rl HIGHEST /f",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            catch { }
        }

        static void LoadConfig(ref int port, ref string page, ref string vdir)
        {
            try
            {
                int parsedPort;
                if (int.TryParse(ConfigurationManager.AppSettings["Port"], out parsedPort) &&
                    parsedPort > 0 && parsedPort < 65536)
                    port = parsedPort;

                string pageText = ConfigurationManager.AppSettings["DefaultPage"];
                if (!string.IsNullOrWhiteSpace(pageText))
                    page = pageText.Trim().TrimStart('/', '\\');

                string vdirText = ConfigurationManager.AppSettings["VirtualDir"];
                if (!string.IsNullOrWhiteSpace(vdirText))
                    vdir = vdirText.Trim();
            }
            catch { }
        }

        static void Handle(HttpListenerContext ctx, WebHost host, string physicalDir, string virtualDir, int port)
        {
            HttpListenerRequest request = ctx.Request;
            HttpListenerResponse response = ctx.Response;

            string rawPath = Uri.UnescapeDataString(request.Url.AbsolutePath);
            string subPath = rawPath;

            if (virtualDir != "/")
            {
                string cleanVDir = virtualDir.EndsWith("/") ? virtualDir : virtualDir + "/";
                if (rawPath.Equals(virtualDir, StringComparison.OrdinalIgnoreCase))
                {
                    response.Redirect(rawPath + "/");
                    response.StatusCode = 302;
                    return;
                }

                if (rawPath.StartsWith(cleanVDir, StringComparison.OrdinalIgnoreCase))
                {
                    subPath = rawPath.Substring(cleanVDir.Length);
                }
                else
                {
                    response.StatusCode = 404;
                    return;
                }
            }
            else
            {
                subPath = rawPath.TrimStart('/');
            }

            string pagePath = subPath;
            string root = Path.GetFullPath(physicalDir);
            string fullPath = Path.GetFullPath(Path.Combine(physicalDir, pagePath.Replace('/', '\\')));

            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = 403;
                return;
            }

            if (string.IsNullOrEmpty(pagePath))
            {
                pagePath = defaultPage;
                fullPath = Path.GetFullPath(Path.Combine(physicalDir, pagePath.Replace('/', '\\')));
            }
            else if (Directory.Exists(fullPath))
            {
                string candidate = Path.Combine(fullPath, defaultPage);
                if (File.Exists(candidate))
                {
                    pagePath = pagePath.TrimEnd('/') + "/" + defaultPage;
                    fullPath = candidate;
                }
            }

            if (IsStaticFile(fullPath) && File.Exists(fullPath))
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                response.ContentType = GetMimeType(Path.GetExtension(fullPath).ToLower());
                response.ContentLength64 = bytes.Length;
                response.OutputStream.Write(bytes, 0, bytes.Length);
                return;
            }

            byte[] postData = null;
            if (request.HasEntityBody)
            {
                using (var ms = new MemoryStream())
                {
                    request.InputStream.CopyTo(ms);
                    postData = ms.ToArray();
                }
            }

            HostResponse res = host.ProcessRequest(
                virtualDir,
                pagePath,
                request.Url.Query,
                request.HttpMethod,
                new NameValueCollection(request.Headers),
                postData,
                request.RemoteEndPoint.Address.ToString(),
                port);

            response.StatusCode = res.StatusCode;
            response.StatusDescription = res.StatusDescription;

            foreach (string[] h in res.Headers)
            {
                string name = h[0];
                string value = h[1];

                if (name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Connection", StringComparison.OrdinalIgnoreCase) ||
                    name.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    response.ContentType = value;
                else if (name.Equals("Location", StringComparison.OrdinalIgnoreCase))
                    response.RedirectLocation = value;
                else
                    response.AppendHeader(name, value);
            }

            if (res.Body != null && res.Body.Length > 0 && res.StatusCode != 204 && res.StatusCode != 304)
            {
                response.ContentLength64 = res.Body.Length;
                response.OutputStream.Write(res.Body, 0, res.Body.Length);
            }
        }

        static bool IsStaticFile(string path)
        {
            switch (Path.GetExtension(path).ToLower())
            {
                case ".css":
                case ".js":
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".svg":
                case ".woff":
                case ".woff2":
                case ".ttf":
                case ".ico":
                case ".mp3":
                case ".mp4":
                case ".map":
                case ".json":
                case ".html":
                case ".htm":
                case ".txt":
                case ".pdf":
                case ".webp":
                    return true;
                default:
                    return false;
            }
        }

        static string GetMimeType(string ext)
        {
            switch (ext)
            {
                case ".css": return "text/css";
                case ".js": return "application/javascript";
                case ".json": case ".map": return "application/json";
                case ".html": case ".htm": return "text/html";
                case ".txt": return "text/plain";
                case ".pdf": return "application/pdf";
                case ".webp": return "image/webp";
                case ".png": return "image/png";
                case ".jpg": case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".svg": return "image/svg+xml";
                case ".ico": return "image/x-icon";
                case ".woff": return "font/woff";
                case ".woff2": return "font/woff2";
                case ".ttf": return "font/ttf";
                case ".mp3": return "audio/mpeg";
                case ".mp4": return "video/mp4";
                default: return "application/octet-stream";
            }
        }
    }
}