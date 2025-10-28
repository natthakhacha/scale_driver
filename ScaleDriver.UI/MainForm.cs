using System;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using ScaleDriver.Core;

namespace ScaleDriver.UI
{
    /// <summary>
    /// Main form for the Scale Driver application
    /// </summary>
    public partial class MainForm : Form
    {
        private ComboBox cmbPortName;
        private ComboBox cmbBaudRate;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnAddParser;
        private Button btnRemoveParser;
        private ListBox lstParsers;
        private TextBox txtWeight;
        private TextBox txtUnit;
        private TextBox txtStatus;
        private TextBox txtRawData;
        private CheckBox chkStable;
        private Label lblLastUpdate;
        private Button btnLoadPlugins;

        private SerialReader _serialReader;
        private FrameManager _frameManager;
        private PluginLoader _pluginLoader;

        public MainForm()
        {
            InitializeComponent();
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _serialReader = new SerialReader();
            _frameManager = new FrameManager();
            _pluginLoader = new PluginLoader();

            // Wire up events
            _serialReader.DataReceived += SerialReader_DataReceived;
            _serialReader.ErrorOccurred += SerialReader_ErrorOccurred;
            _frameManager.WeightParsed += FrameManager_WeightParsed;
            _frameManager.ParseError += FrameManager_ParseError;

            // Load available COM ports
            RefreshPorts();

            // Add some default parsers
            AddDefaultParsers();
        }

        private void InitializeComponent()
        {
            this.Text = "Scale Driver - RS232 Weight Reader";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += MainForm_FormClosing;

            int currentY = 20;
            int leftColumn = 20;
            int rightColumn = 500;

            // Connection Group
            var grpConnection = new GroupBox
            {
                Text = "Connection",
                Location = new Point(leftColumn, currentY),
                Size = new Size(450, 120)
            };

            var lblPort = new Label
            {
                Text = "COM Port:",
                Location = new Point(10, 25),
                Size = new Size(80, 20)
            };
            cmbPortName = new ComboBox
            {
                Location = new Point(100, 23),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblBaudRate = new Label
            {
                Text = "Baud Rate:",
                Location = new Point(10, 55),
                Size = new Size(80, 20)
            };
            cmbBaudRate = new ComboBox
            {
                Location = new Point(100, 53),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbBaudRate.Items.AddRange(new object[] { 2400, 4800, 9600, 19200, 38400, 57600, 115200 });
            cmbBaudRate.SelectedItem = 9600;

            btnConnect = new Button
            {
                Text = "Connect",
                Location = new Point(250, 23),
                Size = new Size(90, 30)
            };
            btnConnect.Click += BtnConnect_Click;

            btnDisconnect = new Button
            {
                Text = "Disconnect",
                Location = new Point(250, 58),
                Size = new Size(90, 30),
                Enabled = false
            };
            btnDisconnect.Click += BtnDisconnect_Click;

            grpConnection.Controls.AddRange(new Control[] { 
                lblPort, cmbPortName, lblBaudRate, cmbBaudRate, btnConnect, btnDisconnect 
            });
            this.Controls.Add(grpConnection);

            currentY += 130;

            // Parsers Group
            var grpParsers = new GroupBox
            {
                Text = "Parsers",
                Location = new Point(leftColumn, currentY),
                Size = new Size(450, 250)
            };

            lstParsers = new ListBox
            {
                Location = new Point(10, 25),
                Size = new Size(340, 180)
            };

            btnAddParser = new Button
            {
                Text = "Add Regex Parser",
                Location = new Point(10, 210),
                Size = new Size(130, 30)
            };
            btnAddParser.Click += BtnAddParser_Click;

            btnRemoveParser = new Button
            {
                Text = "Remove Parser",
                Location = new Point(150, 210),
                Size = new Size(130, 30),
                Enabled = false
            };
            btnRemoveParser.Click += BtnRemoveParser_Click;

            btnLoadPlugins = new Button
            {
                Text = "Load Plugins",
                Location = new Point(290, 210),
                Size = new Size(120, 30)
            };
            btnLoadPlugins.Click += BtnLoadPlugins_Click;

            lstParsers.SelectedIndexChanged += (s, e) => btnRemoveParser.Enabled = lstParsers.SelectedIndex >= 0;

            grpParsers.Controls.AddRange(new Control[] { 
                lstParsers, btnAddParser, btnRemoveParser, btnLoadPlugins 
            });
            this.Controls.Add(grpParsers);

            // Weight Display Group
            var grpWeight = new GroupBox
            {
                Text = "Weight Display",
                Location = new Point(rightColumn, 20),
                Size = new Size(360, 180)
            };

            var lblWeight = new Label
            {
                Text = "Weight:",
                Location = new Point(10, 30),
                Size = new Size(80, 20)
            };
            txtWeight = new TextBox
            {
                Location = new Point(100, 28),
                Size = new Size(150, 25),
                ReadOnly = true,
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right
            };

            var lblUnit = new Label
            {
                Text = "Unit:",
                Location = new Point(10, 65),
                Size = new Size(80, 20)
            };
            txtUnit = new TextBox
            {
                Location = new Point(100, 63),
                Size = new Size(80, 25),
                ReadOnly = true
            };

            chkStable = new CheckBox
            {
                Text = "Stable",
                Location = new Point(100, 95),
                Size = new Size(100, 25),
                Enabled = false
            };

            lblLastUpdate = new Label
            {
                Text = "Last Update: N/A",
                Location = new Point(10, 130),
                Size = new Size(340, 20)
            };

            grpWeight.Controls.AddRange(new Control[] { 
                lblWeight, txtWeight, lblUnit, txtUnit, chkStable, lblLastUpdate 
            });
            this.Controls.Add(grpWeight);

            // Status Group
            var grpStatus = new GroupBox
            {
                Text = "Status",
                Location = new Point(rightColumn, 210),
                Size = new Size(360, 170)
            };

            txtStatus = new TextBox
            {
                Location = new Point(10, 25),
                Size = new Size(340, 135),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            grpStatus.Controls.Add(txtStatus);
            this.Controls.Add(grpStatus);

            currentY += 260;

            // Raw Data Group
            var grpRawData = new GroupBox
            {
                Text = "Raw Data",
                Location = new Point(leftColumn, currentY),
                Size = new Size(840, 250)
            };

            txtRawData = new TextBox
            {
                Location = new Point(10, 25),
                Size = new Size(820, 215),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };

            grpRawData.Controls.Add(txtRawData);
            this.Controls.Add(grpRawData);
        }

        private void RefreshPorts()
        {
            cmbPortName.Items.Clear();
            var ports = SerialPort.GetPortNames();
            
            if (ports.Length == 0)
            {
                // Add a dummy port for testing
                cmbPortName.Items.Add("COM1");
                LogStatus("No COM ports found. Added COM1 for testing.");
            }
            else
            {
                cmbPortName.Items.AddRange(ports);
                LogStatus($"Found {ports.Length} COM port(s)");
            }

            if (cmbPortName.Items.Count > 0)
                cmbPortName.SelectedIndex = 0;
        }

        private void AddDefaultParsers()
        {
            // Generic CSV format: ST,GS,+00012.50,kg
            var csvParser = new RegexParser(
                "Generic CSV",
                @"(?<stable>[A-Z]{2}),(?<mode>[A-Z]{2}),(?<weight>[+-]?\d+\.?\d*),(?<unit>\w+)",
                "weight", "unit", "stable",
                "Generic CSV format with stability, mode, weight, and unit"
            );
            _frameManager.AddParser(csvParser);

            // Simple weight format: 12.50 kg
            var simpleParser = new RegexParser(
                "Simple Weight",
                @"(?<weight>\d+\.?\d*)\s*(?<unit>kg|g|lb|oz)",
                "weight", "unit", null,
                "Simple weight format: value followed by unit"
            );
            _frameManager.AddParser(simpleParser);

            // Toledo format
            var toledoParser = new RegexParser(
                "Toledo Format",
                @"(?<stable>ST|US)\s+(?<weight>[+-]?\d+\.?\d*)\s*(?<unit>\w+)",
                "weight", "unit", "stable",
                "Toledo scale format"
            );
            _frameManager.AddParser(toledoParser);

            RefreshParserList();
        }

        private void RefreshParserList()
        {
            lstParsers.Items.Clear();
            foreach (var parser in _frameManager.Parsers)
            {
                lstParsers.Items.Add($"{parser.Name} - {parser.Description}");
            }
        }

        private void BtnConnect_Click(object? sender, EventArgs e)
        {
            try
            {
                if (cmbPortName.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var portName = cmbPortName.SelectedItem.ToString();
                var baudRate = (int)cmbBaudRate.SelectedItem;

                _serialReader.Open(portName, baudRate);
                _serialReader.StartReading();

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                cmbPortName.Enabled = false;
                cmbBaudRate.Enabled = false;

                LogStatus($"Connected to {portName} at {baudRate} baud");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogStatus($"Connection failed: {ex.Message}");
            }
        }

        private void BtnDisconnect_Click(object? sender, EventArgs e)
        {
            try
            {
                _serialReader.Close();

                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
                cmbPortName.Enabled = true;
                cmbBaudRate.Enabled = true;

                LogStatus("Disconnected");
            }
            catch (Exception ex)
            {
                LogStatus($"Disconnect error: {ex.Message}");
            }
        }

        private void BtnAddParser_Click(object? sender, EventArgs e)
        {
            var form = new FormatEditorForm();
            if (form.ShowDialog() == DialogResult.OK && form.Parser != null)
            {
                _frameManager.AddParser(form.Parser);
                RefreshParserList();
                LogStatus($"Added parser: {form.Parser.Name}");
            }
        }

        private void BtnRemoveParser_Click(object? sender, EventArgs e)
        {
            if (lstParsers.SelectedIndex >= 0)
            {
                var parser = _frameManager.Parsers[lstParsers.SelectedIndex];
                _frameManager.RemoveParser(parser);
                RefreshParserList();
                LogStatus($"Removed parser: {parser.Name}");
            }
        }

        private void BtnLoadPlugins_Click(object? sender, EventArgs e)
        {
            try
            {
                var folderDialog = new FolderBrowserDialog
                {
                    Description = "Select folder containing plugin DLLs"
                };

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    _pluginLoader.AddPluginPath(folderDialog.SelectedPath);
                    var parsers = _pluginLoader.LoadParsers();

                    foreach (var parser in parsers)
                    {
                        _frameManager.AddParser(parser);
                    }

                    RefreshParserList();
                    LogStatus($"Loaded {parsers.Count} parser(s) from plugins");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load plugins: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogStatus($"Plugin load error: {ex.Message}");
            }
        }

        private void SerialReader_DataReceived(object? sender, Core.DataReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SerialReader_DataReceived(sender, e)));
                return;
            }

            LogRawData($"RX: {e.Data}");
            var result = _frameManager.ProcessFrame(e.Data);
        }

        private void SerialReader_ErrorOccurred(object? sender, Core.ErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SerialReader_ErrorOccurred(sender, e)));
                return;
            }

            LogStatus($"Serial error: {e.Exception.Message}");
        }

        private void FrameManager_WeightParsed(object? sender, WeightParsedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => FrameManager_WeightParsed(sender, e)));
                return;
            }

            if (e.Result.Success)
            {
                txtWeight.Text = e.Result.Weight.ToString("F2");
                txtUnit.Text = e.Result.Unit;
                chkStable.Checked = e.Result.IsStable;
                lblLastUpdate.Text = $"Last Update: {DateTime.Now:HH:mm:ss}";
                
                var parserName = e.Parser?.Name ?? "Unknown";
                LogStatus($"Weight parsed by {parserName}: {e.Result.Weight} {e.Result.Unit}");
            }
            else
            {
                LogStatus($"Parse failed: {e.Result.ErrorMessage}");
            }
        }

        private void FrameManager_ParseError(object? sender, ParseErrorEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => FrameManager_ParseError(sender, e)));
                return;
            }

            LogStatus($"Parse error ({e.Parser?.Name}): {e.Exception.Message}");
        }

        private void LogStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogStatus(message)));
                return;
            }

            txtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            txtStatus.SelectionStart = txtStatus.Text.Length;
            txtStatus.ScrollToCaret();
        }

        private void LogRawData(string data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogRawData(data)));
                return;
            }

            txtRawData.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {data}\r\n");
            txtRawData.SelectionStart = txtRawData.Text.Length;
            txtRawData.ScrollToCaret();

            // Keep only last 100 lines
            var lines = txtRawData.Lines;
            if (lines.Length > 100)
            {
                txtRawData.Lines = lines.Skip(lines.Length - 100).ToArray();
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_serialReader.IsConnected)
            {
                _serialReader.Close();
            }
            _serialReader.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serialReader?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
