using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using MISS.Browser.Models;
using MISS.Browser.Services;
using System.IO;

namespace MISS.Browser.Forms
{
    public partial class MainBrowserForm : Form
    {
        private NetworkingService _networkingService = new NetworkingService();
        private ServerControlService _serverControl;
        private Dictionary<string, (string ip, int port)> _dnsCache = new Dictionary<string, (string, int)>();
        private List<string> _history = new List<string>();
        
        // UI Controls
        private TextBox _addressBar = null!;
        private Button _btnGo = null!;
        private WebBrowser _displayArea = null!;
        private ListBox _logBox = null!;
        private Panel _headerPanel = null!;
        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _statusLabel = null!;
        private TabControl _mainTabs = null!;

        public MainBrowserForm()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            _serverControl = new ServerControlService(Path.GetFullPath(Path.Combine(basePath, "../../../..")));
            
            SetupUI();
            this.Text = "MISS - Mini Internet Simulation System";
            this.Size = new Size(1200, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormClosing += (s, e) => _serverControl.StopAll();
        }

        private void SetupUI()
        {
            _mainTabs = new TabControl { 
                Dock = DockStyle.Fill, 
                Padding = new Point(20, 10),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            
            TabPage browserTab = new TabPage { Text = "🌐 Web Browser", BackColor = Color.FromArgb(30, 30, 30) };
            TabPage settingsTab = new TabPage { Text = "⚙️ Server Settings", BackColor = Color.FromArgb(30, 30, 30) };
            _mainTabs.TabPages.Add(browserTab);
            _mainTabs.TabPages.Add(settingsTab);

            #region Browser Tab Setup
            _headerPanel = new Panel { 
                Dock = DockStyle.Top, 
                Height = 80, 
                BackColor = Color.FromArgb(45, 45, 48), 
                Padding = new Padding(20) 
            };
            
            Label logo = new Label { 
                Text = "MISS", 
                Font = new Font("Segoe UI Semibold", 22, FontStyle.Bold), 
                ForeColor = Color.FromArgb(0, 170, 255), 
                AutoSize = true, 
                Left = 20, 
                Top = 15 
            };
            _headerPanel.Controls.Add(logo);

            _addressBar = new TextBox { 
                Left = 140, 
                Top = 22, 
                Width = 750, 
                Font = new Font("Segoe UI", 13), 
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };
            _addressBar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Navigate(); };
            _headerPanel.Controls.Add(_addressBar);

            _btnGo = new Button { 
                Text = "GO", 
                Left = 905, 
                Top = 20, 
                Width = 100, 
                Height = 36, 
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.FromArgb(0, 122, 204), 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnGo.FlatAppearance.BorderSize = 0;
            _btnGo.Click += (s, e) => Navigate();
            _headerPanel.Controls.Add(_btnGo);

            browserTab.Controls.Add(_headerPanel);

            SplitContainer splitContainer = new SplitContainer { 
                Dock = DockStyle.Fill, 
                Orientation = Orientation.Horizontal, 
                SplitterDistance = 500, 
                Top = 80 
            };
            
            Panel browserContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1) };
            _displayArea = new WebBrowser { Dock = DockStyle.Fill, ScriptErrorsSuppressed = true };
            browserContainer.Controls.Add(_displayArea);
            splitContainer.Panel1.Controls.Add(browserContainer);

            Panel logsPanel = new Panel { 
                Dock = DockStyle.Fill, 
                BackColor = Color.FromArgb(20, 20, 20), 
                Padding = new Padding(10) 
            };
            Label logTitle = new Label { 
                Text = "🛰️ LIVE NETWORK TRAFFIC LOGS", 
                ForeColor = Color.FromArgb(180, 180, 180), 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                Dock = DockStyle.Top, 
                Height = 30 
            };
            logsPanel.Controls.Add(logTitle);
            _logBox = new ListBox { 
                Dock = DockStyle.Fill, 
                BackColor = Color.FromArgb(20, 20, 20), 
                ForeColor = Color.FromArgb(0, 255, 127), 
                Font = new Font("Consolas", 11), 
                BorderStyle = BorderStyle.None 
            };
            logsPanel.Controls.Add(_logBox);
            splitContainer.Panel2.Controls.Add(logsPanel);
            browserTab.Controls.Add(splitContainer);
            splitContainer.BringToFront();
            #endregion

            #region Settings Tab Setup
            FlowLayoutPanel settingsPanel = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, 
                Padding = new Padding(40), 
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true
            };
            
            Label settingsHeader = new Label { 
                Text = "Global Server Management", 
                Font = new Font("Segoe UI Light", 24), 
                ForeColor = Color.White, 
                AutoSize = true, 
                Margin = new Padding(0, 0, 0, 30) 
            };
            settingsPanel.Controls.Add(settingsHeader);

            AddServerControl(settingsPanel, "DNS Resolver", "Servers/DNS_Server/DNS_Server.exe", "Handles domain queries (UDP 5053)");
            AddServerControl(settingsPanel, "Apple Website", "Websites/apple/apple_server.exe", "Port 8081 - Main content server");
            AddServerControl(settingsPanel, "Google Website", "Websites/google/google_server.exe", "Port 8082 - Search engine simulation");
            AddServerControl(settingsPanel, "GitHub Website", "Websites/github/github_server.exe", "Port 8083 - Repository simulation");

            settingsTab.Controls.Add(settingsPanel);
            #endregion

            this.Controls.Add(_mainTabs);

            _statusStrip = new StatusStrip { BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White };
            _statusLabel = new ToolStripStatusLabel { Text = "Ready" };
            _statusStrip.Items.Add(_statusLabel);
            this.Controls.Add(_statusStrip);
        }

        private void AddServerControl(FlowLayoutPanel panel, string serverName, string exePath, string description)
        {
            Panel p = new Panel { 
                Width = 700, 
                Height = 100, 
                BackColor = Color.FromArgb(45, 45, 48), 
                Margin = new Padding(0, 0, 0, 20),
                Padding = new Padding(15)
            };
            
            Label lbl = new Label { 
                Text = serverName, 
                Font = new Font("Segoe UI Semibold", 14), 
                ForeColor = Color.White,
                Left = 20, 
                Top = 15, 
                AutoSize = true 
            };
            
            Label desc = new Label { 
                Text = description, 
                Font = new Font("Segoe UI", 9), 
                ForeColor = Color.FromArgb(180, 180, 180),
                Left = 22, 
                Top = 45, 
                AutoSize = true 
            };

            Label status = new Label { 
                Text = "● OFFLINE", 
                ForeColor = Color.FromArgb(255, 80, 80), 
                Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                Left = 450, 
                Top = 38, 
                Width = 100 
            };
            
            Button btn = new Button { 
                Text = "START SERVER", 
                Left = 550, 
                Top = 28, 
                Width = 130, 
                Height = 40, 
                FlatStyle = FlatStyle.Flat, 
                BackColor = Color.FromArgb(0, 170, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += (s, e) => {
                if (_serverControl.IsRunning(serverName)) {
                    _serverControl.StopServer(serverName);
                    btn.Text = "START SERVER";
                    btn.BackColor = Color.FromArgb(0, 170, 255);
                    status.Text = "● OFFLINE";
                    status.ForeColor = Color.FromArgb(255, 80, 80);
                } else {
                    _serverControl.StartServer(serverName, exePath);
                    btn.Text = "STOP SERVER";
                    btn.BackColor = Color.FromArgb(255, 80, 80);
                    status.Text = "● ONLINE";
                    status.ForeColor = Color.FromArgb(0, 255, 127);
                }
            };

            p.Controls.Add(lbl);
            p.Controls.Add(desc);
            p.Controls.Add(status);
            p.Controls.Add(btn);
            panel.Controls.Add(p);
        }

        private async void Navigate()
        {
            string domain = _addressBar.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(domain)) return;

            _btnGo.Enabled = false;
            _statusLabel.Text = $"Resolving {domain}...";
            Log("BROWSER", "Action", $"Navigating to {domain}", "");

            try
            {
                string ip;
                int port;

                // 1. DNS Resolution
                Log("UDP", "Request", $"Sending DNS query for {domain} to 127.0.0.1:5053", "");
                var result = await _networkingService.ResolveDNSAsync(domain);
                
                if (result.ip == "NOT_FOUND")
                {
                    Log("UDP", "Response", $"Domain {domain} not found in DNS", "");
                    ShowError("Domain Not Found", $"The DNS server does not have a record for '{domain}'.");
                    return;
                }

                ip = result.ip!;
                port = result.port;
                Log("UDP", "Response", $"DNS resolved {domain} to {ip}:{port}", "");

                // 2. TCP Page Retrieval
                Log("TCP", "Connect", $"Connecting to {ip}:{port}...", "");
                string html = await _networkingService.FetchWebPageAsync(ip, port);
                
                Log("TCP", "Response", "Received HTML content", "");
                _displayArea.DocumentText = html;
                _statusLabel.Text = "Done";
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message switch {
                    "DNS_OFFLINE" => "DNS Server Not Responding",
                    "DNS_TIMEOUT" => "DNS Request Timed Out",
                    "SERVER_OFFLINE" => "Website Server Offline",
                    "SERVER_TIMEOUT" => "Website Not Responding",
                    _ => "Connection Error"
                };

                Log("ERROR", ex.Message, errorMsg, "");
                ShowError(errorMsg, $"Details: {ex.Message}");
            }
            finally
            {
                _btnGo.Enabled = true;
            }
        }

        private void ShowError(string title, string body)
        {
            string html = $@"
                <html><body style='font-family:sans-serif; text-align:center; padding-top:50px; background-color:#fff5f5;'>
                    <h1 style='color:#e53e3e;'>{title}</h1>
                    <p style='color:#718096;'>{body}</p>
                    <hr style='width:50%; margin:20px auto;'>
                    <p style='font-size:0.8em; color:#a0aec0;'>MISS - Mini Internet Simulation System</p>
                </body></html>";
            _displayArea.DocumentText = html;
            _statusLabel.Text = title;
        }

        private void Log(string protocol, string action, string message, string details)
        {
            _logBox.Invoke((MethodInvoker)delegate {
                string logLine = $"[{DateTime.Now:HH:mm:ss}] {protocol,-8} | {action,-10} | {message}";
                _logBox.Items.Add(logLine);
                _logBox.SelectedIndex = _logBox.Items.Count - 1;
            });
        }
    }
}
