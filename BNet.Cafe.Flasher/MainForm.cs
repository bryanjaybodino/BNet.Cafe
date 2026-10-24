using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace BNet.Cafe.Flasher
{
    public partial class MainForm : Form
    {
        private sealed class PortInfo
        {
            public string Address;
            public string BoardName;
            public string Fqbn;

            public override string ToString()
            {
                return string.IsNullOrEmpty(BoardName)
                    ? Address + "  (unknown board)"
                    : Address + "  (" + BoardName + ")";
            }
        }

        private CancellationTokenSource _cts;
        private bool _detecting;
        private bool _busy;
        private bool _loading;
        private bool _dirty;
        private string _loadedPath;

        public MainForm()
        {
            InitializeComponent();
            InitializeData();
        }

        private void InitializeData()
        {
            txtEditor.TextChanged += txtEditor_TextChanged;
        }

        /// <summary>arduino-cli.exe always sits next to the application (e.g. bin\Debug).</summary>
        private static string CliPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "arduino-cli.exe"); }
        }

        // ------------------------------------------------------------ Event handlers

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!File.Exists(CliPath))
            {
                Log("arduino-cli.exe was not found in " + AppDomain.CurrentDomain.BaseDirectory, true);
                lblStatus.Text = "arduino-cli.exe missing";
                return;
            }

            await DetectBoardsAsync(true);
        }

        private void btnSketch_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardChanges()) return;

            using (var d = new OpenFileDialog { Filter = "Arduino sketch (*.ino)|*.ino" })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                    LoadSketch(d.FileName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSketch();
        }

        private void txtEditor_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            _dirty = true;
            UpdateEditorUi();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                if (_loadedPath != null && !_busy) SaveSketch();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await DetectBoardsAsync(false);
        }

        private void cmbPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            var port = cmbPort.SelectedItem as PortInfo;
            if (port != null) ApplyDetectedBoard(port);
        }

        private async void btnCompile_Click(object sender, EventArgs e)
        {
            await BuildAsync(false);
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            await BuildAsync(true);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_cts != null) _cts.Cancel();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmDiscardChanges())
            {
                e.Cancel = true;
                return;
            }
            if (_cts != null) _cts.Cancel();
            base.OnFormClosing(e);
        }

        // ------------------------------------------------------------ Sketch editor

        private void LoadSketch(string path)
        {
            try
            {
                var text = File.ReadAllText(path, Encoding.UTF8);

                _loading = true;
                txtEditor.ReadOnly = false;
                txtEditor.LoadText(text);
                _loading = false;

                _loadedPath = path;
                _dirty = false;
                txtSketch.Text = path;
                txtEditor.Enabled = true;
                UpdateEditorUi();
            }
            catch (Exception ex)
            {
                _loading = false;
                Warn("Could not open the file: " + ex.Message);
            }
        }

        private bool SaveSketch()
        {
            if (_loadedPath == null) return false;
            try
            {
                File.WriteAllText(_loadedPath, txtEditor.Text, new UTF8Encoding(false));
                _dirty = false;
                UpdateEditorUi();
                lblStatus.Text = "Saved " + Path.GetFileName(_loadedPath);
                return true;
            }
            catch (Exception ex)
            {
                Warn("Could not save the file: " + ex.Message);
                return false;
            }
        }

        /// <summary>Returns false if the user cancelled. Offers to save unsaved edits.</summary>
        private bool ConfirmDiscardChanges()
        {
            if (!_dirty || _loadedPath == null) return true;

            var r = MessageBox.Show(this,
                "Save changes to " + Path.GetFileName(_loadedPath) + "?",
                Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (r == DialogResult.Cancel) return false;
            if (r == DialogResult.Yes) return SaveSketch();
            return true; // discard
        }

        private void UpdateEditorUi()
        {
            lblEditor.Text = _loadedPath == null
                ? "Sketch editor - open a .ino file to edit it"
                : "Sketch editor - " + Path.GetFileName(_loadedPath) + (_dirty ? "  *(unsaved)" : "");
            btnSave.Enabled = _loadedPath != null && !_busy;
        }

        // ------------------------------------------------------------ Board detection

        private async Task DetectBoardsAsync(bool silent)
        {
            if (_detecting) return;
            _detecting = true;
            btnRefresh.Enabled = false;
            lblStatus.Text = "Detecting boards...";

            var ports = new List<PortInfo>();
            bool cliOk = false;

            try
            {
                string json = await CaptureAsync(CliPath, "board list --format json", 20000);
                ports = ParsePorts(json);
                cliOk = true;
            }
            catch (Exception ex)
            {
                if (!silent)
                    Log("Board detection via arduino-cli failed: " + ex.Message, true);
            }

            // Fallback: plain list of serial ports (board must be chosen manually)
            if (!cliOk)
            {
                try
                {
                    foreach (var name in SerialPort.GetPortNames())
                        ports.Add(new PortInfo { Address = name });
                }
                catch (Exception ex)
                {
                    Log("Could not list ports: " + ex.Message, true);
                }
            }

            ports = ports
                .OrderBy(p => p.Address.Length)
                .ThenBy(p => p.Address)
                .ToList();

            var previous = cmbPort.SelectedItem as PortInfo;
            cmbPort.Items.Clear();
            foreach (var p in ports) cmbPort.Items.Add(p);

            if (ports.Count > 0)
            {
                // Keep the previous port if it is still there, otherwise prefer a port with a known board
                PortInfo pick = null;
                if (previous != null)
                    pick = ports.FirstOrDefault(p => p.Address == previous.Address);
                if (pick == null)
                    pick = ports.FirstOrDefault(p => p.Fqbn != null) ?? ports[0];
                cmbPort.SelectedItem = pick;
                ApplyDetectedBoard(pick);
            }
            else
            {
                lblStatus.Text = "No board found. Plug in the board and click Detect Board.";
            }

            btnRefresh.Enabled = true;
            _detecting = false;
        }

        private static List<PortInfo> ParsePorts(string json)
        {
            var result = new List<PortInfo>();
            if (string.IsNullOrWhiteSpace(json)) return result;

            var ser = new JavaScriptSerializer();
            object root = ser.DeserializeObject(json);

            object[] items = null;
            var dict = root as Dictionary<string, object>;
            if (dict != null)
            {
                object dp;
                if (dict.TryGetValue("detected_ports", out dp)) items = dp as object[];
            }
            else
            {
                items = root as object[]; // older arduino-cli versions
            }
            if (items == null) return result;

            foreach (var o in items)
            {
                var item = o as Dictionary<string, object>;
                if (item == null) continue;

                object portObj;
                if (!item.TryGetValue("port", out portObj)) continue;
                var port = portObj as Dictionary<string, object>;
                if (port == null) continue;

                var address = GetString(port, "address");
                var protocol = GetString(port, "protocol");
                if (string.IsNullOrEmpty(address)) continue;
                if (!string.IsNullOrEmpty(protocol) && protocol != "serial") continue;

                var info = new PortInfo { Address = address };

                object mbObj;
                if (item.TryGetValue("matching_boards", out mbObj))
                {
                    var boards = mbObj as object[];
                    if (boards != null && boards.Length > 0)
                    {
                        var first = boards[0] as Dictionary<string, object>;
                        if (first != null)
                        {
                            info.BoardName = GetString(first, "name");
                            info.Fqbn = GetString(first, "fqbn");
                        }
                    }
                }
                result.Add(info);
            }
            return result;
        }

        private static string GetString(Dictionary<string, object> d, string key)
        {
            object v;
            return d.TryGetValue(key, out v) && v != null ? v.ToString() : null;
        }

        private void ApplyDetectedBoard(PortInfo port)
        {
            if (string.IsNullOrEmpty(port.Fqbn))
                lblStatus.Text = port.Address + ": board not recognised.";
            else
                lblStatus.Text = "Detected " + (port.BoardName ?? port.Fqbn) + " on " + port.Address;
        }

        private string SelectedPortAddress()
        {
            var p = cmbPort.SelectedItem as PortInfo;
            return p != null ? p.Address : null;
        }

        /// <summary>FQBN of the board detected on the selected port (null if unknown).</summary>
        private string CurrentFqbn()
        {
            var p = cmbPort.SelectedItem as PortInfo;
            return p != null ? p.Fqbn : null;
        }

        // ------------------------------------------------------------ Build / upload

        private async Task BuildAsync(bool upload)
        {
            if (_loadedPath == null || !File.Exists(_loadedPath))
            {
                Warn("Please open a .ino file first.");
                return;
            }

            // Make sure the edits in the editor are what gets compiled
            if (_dirty)
            {
                if (!SaveSketch()) return;
                Log("Saved changes to " + Path.GetFileName(_loadedPath));
            }

            var port = SelectedPortAddress();
            if (string.IsNullOrWhiteSpace(port))
            {
                Warn("No board found. Plug in the board and click Detect Board.");
                return;
            }

            var fqbn = CurrentFqbn();
            if (string.IsNullOrWhiteSpace(fqbn))
            {
                Warn("The board on " + port + " was not recognised, so it cannot be compiled for.");
                return;
            }

            string tempRoot;
            string sketchDir = PrepareSketchFolder(_loadedPath, out tempRoot);

            try
            {
                await RunJobAsync(async ct =>
                {
                    Log("> compiling for " + fqbn);
                    int rc = await RunProcessAsync(CliPath,
                        "compile --fqbn \"" + fqbn + "\" \"" + sketchDir + "\"", ct);
                    if (rc != 0 || !upload) return rc;

                    Log("> uploading to " + port);
                    return await RunProcessAsync(CliPath,
                        "upload -p \"" + port + "\" --fqbn \"" + fqbn + "\" \"" + sketchDir + "\"", ct);
                });
            }
            finally
            {
                if (tempRoot != null)
                {
                    try { Directory.Delete(tempRoot, true); } catch { /* ignore */ }
                }
            }
        }

        /// <summary>
        /// arduino-cli requires the sketch folder name to equal the .ino name.
        /// If it doesn't, the sketch files are copied to a temp folder with the right name.
        /// </summary>
        private static string PrepareSketchFolder(string inoPath, out string tempRoot)
        {
            tempRoot = null;
            var dir = Path.GetDirectoryName(inoPath);
            var name = Path.GetFileNameWithoutExtension(inoPath);

            if (string.Equals(new DirectoryInfo(dir).Name, name, StringComparison.OrdinalIgnoreCase))
                return dir;

            tempRoot = Path.Combine(Path.GetTempPath(), "BNetFlasher_" + Guid.NewGuid().ToString("N"));
            var target = Path.Combine(tempRoot, name);
            Directory.CreateDirectory(target);

            string[] allowed = { ".ino", ".h", ".hpp", ".c", ".cpp", ".s", ".json" };
            foreach (var f in Directory.GetFiles(dir))
            {
                var ext = Path.GetExtension(f).ToLowerInvariant();
                if (allowed.Contains(ext))
                    File.Copy(f, Path.Combine(target, Path.GetFileName(f)), true);
            }
            return target;
        }

        private async Task RunJobAsync(Func<CancellationToken, Task<int>> job)
        {
            SetBusy(true);
            _cts = new CancellationTokenSource();
            try
            {
                int rc = await job(_cts.Token);
                if (rc == 0)
                {
                    Log(Environment.NewLine + "=== SUCCESS ===", false, Color.LimeGreen);
                    lblStatus.Text = "Success";
                }
                else
                {
                    Log(Environment.NewLine + "=== FAILED (exit code " + rc + ") ===", true);
                    lblStatus.Text = "Failed";
                }
            }
            catch (OperationCanceledException)
            {
                Log(Environment.NewLine + "=== CANCELLED ===", true);
                lblStatus.Text = "Cancelled";
            }
            catch (Win32Exception)
            {
                Log("Could not start arduino-cli.exe. Make sure it is in the application folder.", true);
                lblStatus.Text = "arduino-cli not found";
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message, true);
                lblStatus.Text = "Error";
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
                SetBusy(false);
            }
        }

        // ------------------------------------------------------------ Process helpers

        /// <summary>Runs a short command and returns its standard output.</summary>
        private static Task<string> CaptureAsync(string exe, string args, int timeoutMs)
        {
            return Task.Run(() =>
            {
                var psi = new ProcessStartInfo(exe, args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    var outTask = p.StandardOutput.ReadToEndAsync();
                    var errTask = p.StandardError.ReadToEndAsync();

                    if (!p.WaitForExit(timeoutMs))
                    {
                        try { p.Kill(); } catch { }
                        throw new TimeoutException("arduino-cli did not respond in time.");
                    }
                    p.WaitForExit();

                    if (p.ExitCode != 0)
                        throw new InvalidOperationException(errTask.Result.Trim());

                    return outTask.Result;
                }
            });
        }

        private Task<int> RunProcessAsync(string exe, string args, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<int>();

            var psi = new ProcessStartInfo(exe, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.OutputDataReceived += (s, e) => { if (e.Data != null) Log(e.Data); };
            p.ErrorDataReceived += (s, e) => { if (e.Data != null) Log(e.Data, true); };

            CancellationTokenRegistration reg = default(CancellationTokenRegistration);

            p.Exited += (s, e) =>
            {
                p.WaitForExit(); // flush remaining async output
                int code = p.ExitCode;
                reg.Dispose();
                p.Dispose();

                if (ct.IsCancellationRequested) tcs.TrySetCanceled();
                else tcs.TrySetResult(code);
            };

            p.Start(); // throws Win32Exception if the executable is not found
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            int pid = p.Id;
            reg = ct.Register(() => KillProcessTree(pid));

            return tcs.Task;
        }

        private static void KillProcessTree(int pid)
        {
            try
            {
                var psi = new ProcessStartInfo("taskkill", "/PID " + pid + " /T /F")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(psi);
            }
            catch { /* process may already be gone */ }
        }

        // ------------------------------------------------------------ UI helpers

        private void SetBusy(bool busy)
        {
            Control[] toggled =
            {
                btnCompile, btnUpload, btnRefresh, btnSketch,
                cmbPort
            };
            foreach (var c in toggled) c.Enabled = !busy;

            _busy = busy;
            txtEditor.ReadOnly = busy;
            UpdateEditorUi();

            btnCancel.Enabled = busy;
            progress.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            if (busy) lblStatus.Text = "Working...";
        }

        private void Warn(string msg)
        {
            MessageBox.Show(this, msg, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Log(string text, bool error = false, Color? color = null)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Log(text, error, color)));
                return;
            }

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionColor = color ?? (error ? Color.OrangeRed : Color.Gainsboro);
            txtLog.AppendText(text + Environment.NewLine);
            txtLog.ScrollToCaret();
        }
    }
}