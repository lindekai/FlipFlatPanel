// ============================================================
// FlipFlat Panel - ASCOM Setup Dialog
// ============================================================
// Ersetzt die generierte SetupDialogForm.cs im ASCOM-Projekt.
// Enthält COM-Port-Auswahl und die neuen v3.0 Einstellungen:
//   - Servo Open/Close Position (500-2500 µs)
//   - Geschwindigkeit (1-10)
//   - SoftClose Modus
// ============================================================

using ASCOM.Utilities;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.FlipFlatPanelV2.CoverCalibrator
{
    [ComVisible(false)]
    public partial class SetupDialogForm : Form
    {
        TraceLogger tl;

        // Controls
        private ComboBox cmbComPort;
        private CheckBox chkTrace;
        private NumericUpDown nudOpenPos;
        private NumericUpDown nudClosePos;
        private TrackBar trkSpeed;
        private Label lblSpeedValue;
        private CheckBox chkSoftClose;
        private Button btnOK;
        private Button btnCancel;
        private Button btnTestOpen;
        private Button btnTestClose;
        private Button btnDefaults;

        public SetupDialogForm(TraceLogger tlDriver)
        {
            tl = tlDriver;
            InitializeFormControls();
            LoadCurrentSettings();
        }

        private void InitializeFormControls()
        {
            // Form
            this.Text = "FlipFlat Panel - Einstellungen";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(420, 480);
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.ForeColor = Color.FromArgb(224, 224, 224);
            this.Font = new Font("Segoe UI", 9f);

            int y = 15;
            int labelX = 15;
            int controlX = 170;
            int controlWidth = 220;

            // === COM-Port ===
            AddLabel("COM-Port:", labelX, y + 3);
            cmbComPort = new ComboBox();
            cmbComPort.Location = new Point(controlX, y);
            cmbComPort.Size = new Size(controlWidth, 25);
            cmbComPort.DropDownStyle = ComboBoxStyle.DropDownList;
            string[] ports = SerialPort.GetPortNames();
            cmbComPort.Items.AddRange(ports);
            this.Controls.Add(cmbComPort);
            y += 35;

            // === Separator ===
            AddSeparator(y, "Servo-Endpunkte");
            y += 30;

            // === Open Position ===
            AddLabel("Open Position (µs):", labelX, y + 3);
            nudOpenPos = new NumericUpDown();
            nudOpenPos.Location = new Point(controlX, y);
            nudOpenPos.Size = new Size(100, 25);
            nudOpenPos.Minimum = 500;
            nudOpenPos.Maximum = 2500;
            nudOpenPos.Increment = 10;
            nudOpenPos.Value = 2500;
            nudOpenPos.BackColor = Color.FromArgb(42, 42, 60);
            nudOpenPos.ForeColor = Color.FromArgb(224, 224, 224);
            this.Controls.Add(nudOpenPos);

            btnTestOpen = new Button();
            btnTestOpen.Text = "Test";
            btnTestOpen.Location = new Point(controlX + 110, y);
            btnTestOpen.Size = new Size(60, 25);
            btnTestOpen.FlatStyle = FlatStyle.Flat;
            btnTestOpen.ForeColor = Color.FromArgb(91, 155, 213);
            btnTestOpen.Click += BtnTestOpen_Click;
            this.Controls.Add(btnTestOpen);
            y += 35;

            // === Close Position ===
            AddLabel("Close Position (µs):", labelX, y + 3);
            nudClosePos = new NumericUpDown();
            nudClosePos.Location = new Point(controlX, y);
            nudClosePos.Size = new Size(100, 25);
            nudClosePos.Minimum = 500;
            nudClosePos.Maximum = 2500;
            nudClosePos.Increment = 10;
            nudClosePos.Value = 540;
            nudClosePos.BackColor = Color.FromArgb(42, 42, 60);
            nudClosePos.ForeColor = Color.FromArgb(224, 224, 224);
            this.Controls.Add(nudClosePos);

            btnTestClose = new Button();
            btnTestClose.Text = "Test";
            btnTestClose.Location = new Point(controlX + 110, y);
            btnTestClose.Size = new Size(60, 25);
            btnTestClose.FlatStyle = FlatStyle.Flat;
            btnTestClose.ForeColor = Color.FromArgb(91, 155, 213);
            btnTestClose.Click += BtnTestClose_Click;
            this.Controls.Add(btnTestClose);
            y += 35;

            // Hinweis
            Label lblHint = new Label();
            lblHint.Text = "Tipp: Werte in 10er-Schritten ändern und mit Test prüfen.";
            lblHint.Location = new Point(labelX, y);
            lblHint.Size = new Size(380, 18);
            lblHint.ForeColor = Color.FromArgb(136, 136, 153);
            lblHint.Font = new Font("Segoe UI", 8f);
            this.Controls.Add(lblHint);
            y += 25;

            // === Separator ===
            AddSeparator(y, "Bewegung");
            y += 30;

            // === Speed ===
            AddLabel("Geschwindigkeit:", labelX, y + 3);
            trkSpeed = new TrackBar();
            trkSpeed.Location = new Point(controlX, y);
            trkSpeed.Size = new Size(170, 30);
            trkSpeed.Minimum = 1;
            trkSpeed.Maximum = 10;
            trkSpeed.Value = 5;
            trkSpeed.TickFrequency = 1;
            trkSpeed.SmallChange = 1;
            trkSpeed.LargeChange = 2;
            trkSpeed.ValueChanged += TrkSpeed_ValueChanged;
            this.Controls.Add(trkSpeed);

            lblSpeedValue = new Label();
            lblSpeedValue.Text = "5";
            lblSpeedValue.Location = new Point(controlX + 180, y + 3);
            lblSpeedValue.Size = new Size(30, 20);
            lblSpeedValue.ForeColor = Color.FromArgb(91, 155, 213);
            lblSpeedValue.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            this.Controls.Add(lblSpeedValue);
            y += 40;

            // Speed-Labels
            Label lblSlow = new Label();
            lblSlow.Text = "Langsam";
            lblSlow.Location = new Point(controlX, y);
            lblSlow.Size = new Size(60, 15);
            lblSlow.ForeColor = Color.FromArgb(136, 136, 153);
            lblSlow.Font = new Font("Segoe UI", 7.5f);
            this.Controls.Add(lblSlow);

            Label lblFast = new Label();
            lblFast.Text = "Schnell";
            lblFast.Location = new Point(controlX + 130, y);
            lblFast.Size = new Size(60, 15);
            lblFast.ForeColor = Color.FromArgb(136, 136, 153);
            lblFast.Font = new Font("Segoe UI", 7.5f);
            lblFast.TextAlign = ContentAlignment.TopRight;
            this.Controls.Add(lblFast);
            y += 25;

            // === SoftClose ===
            chkSoftClose = new CheckBox();
            chkSoftClose.Text = "SoftClose (sanftes Starten und Stoppen)";
            chkSoftClose.Location = new Point(labelX, y);
            chkSoftClose.Size = new Size(380, 25);
            chkSoftClose.ForeColor = Color.FromArgb(224, 224, 224);
            this.Controls.Add(chkSoftClose);
            y += 35;

            // === Separator ===
            AddSeparator(y, "Optionen");
            y += 30;

            // === Trace ===
            chkTrace = new CheckBox();
            chkTrace.Text = "Trace-Logging aktivieren (für Fehlersuche)";
            chkTrace.Location = new Point(labelX, y);
            chkTrace.Size = new Size(380, 25);
            chkTrace.ForeColor = Color.FromArgb(224, 224, 224);
            this.Controls.Add(chkTrace);
            y += 35;

            // === Defaults Button ===
            btnDefaults = new Button();
            btnDefaults.Text = "Standardwerte";
            btnDefaults.Location = new Point(labelX, y);
            btnDefaults.Size = new Size(110, 30);
            btnDefaults.FlatStyle = FlatStyle.Flat;
            btnDefaults.ForeColor = Color.FromArgb(136, 136, 153);
            btnDefaults.Click += BtnDefaults_Click;
            this.Controls.Add(btnDefaults);

            // === OK / Cancel ===
            btnCancel = new Button();
            btnCancel.Text = "Abbrechen";
            btnCancel.Location = new Point(195, y);
            btnCancel.Size = new Size(90, 30);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.ForeColor = Color.FromArgb(255, 107, 107);
            btnCancel.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancel);

            btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Location = new Point(295, y);
            btnOK.Size = new Size(95, 30);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.ForeColor = Color.FromArgb(107, 203, 119);
            btnOK.DialogResult = DialogResult.OK;
            this.Controls.Add(btnOK);

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.Size = new Size(150, 20);
            lbl.ForeColor = Color.FromArgb(224, 224, 224);
            this.Controls.Add(lbl);
        }

        private void AddSeparator(int y, string title)
        {
            Label lbl = new Label();
            lbl.Text = title.ToUpper();
            lbl.Location = new Point(15, y);
            lbl.Size = new Size(380, 18);
            lbl.ForeColor = Color.FromArgb(91, 155, 213);
            lbl.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            this.Controls.Add(lbl);

            Label line = new Label();
            line.Location = new Point(15, y + 18);
            line.Size = new Size(375, 1);
            line.BackColor = Color.FromArgb(58, 58, 76);
            this.Controls.Add(line);
        }

        private void LoadCurrentSettings()
        {
            // COM-Port
            if (cmbComPort.Items.Contains(CoverCalibratorHardware.comPort))
                cmbComPort.SelectedItem = CoverCalibratorHardware.comPort;
            else if (cmbComPort.Items.Count > 0)
                cmbComPort.SelectedIndex = 0;

            // Trace
            chkTrace = chkTrace ?? new CheckBox();
            chkTrace.Checked = tl.Enabled;

            // Servo-Einstellungen aus Profil lesen (oder Defaults)
            try
            {
                using (var profile = new Profile())
                {
                    profile.DeviceType = "CoverCalibrator";
                    string progId = CoverCalibrator.DriverProgId;

                    string openStr = profile.GetValue(progId, "Servo Open Position", string.Empty, "2500");
                    string closeStr = profile.GetValue(progId, "Servo Close Position", string.Empty, "540");
                    string speedStr = profile.GetValue(progId, "Servo Speed", string.Empty, "5");
                    string softStr = profile.GetValue(progId, "SoftClose", string.Empty, "false");

                    nudOpenPos.Value = Math.Max(500, Math.Min(2500, int.Parse(openStr)));
                    nudClosePos.Value = Math.Max(500, Math.Min(2500, int.Parse(closeStr)));
                    trkSpeed.Value = Math.Max(1, Math.Min(10, int.Parse(speedStr)));
                    chkSoftClose.Checked = bool.Parse(softStr);
                }
            }
            catch
            {
                // Defaults bei Fehler
                nudOpenPos.Value = 2500;
                nudClosePos.Value = 540;
                trkSpeed.Value = 5;
                chkSoftClose.Checked = false;
            }

            lblSpeedValue.Text = trkSpeed.Value.ToString();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                // Werte in die Hardware-Klasse übernehmen
                CoverCalibratorHardware.comPort = cmbComPort.SelectedItem?.ToString() ?? "COM1";
                tl.Enabled = chkTrace.Checked;

                // Servo-Einstellungen ins Profil schreiben
                try
                {
                    using (var profile = new Profile())
                    {
                        profile.DeviceType = "CoverCalibrator";
                        string progId = CoverCalibrator.DriverProgId;

                        profile.WriteValue(progId, "Servo Open Position", nudOpenPos.Value.ToString());
                        profile.WriteValue(progId, "Servo Close Position", nudClosePos.Value.ToString());
                        profile.WriteValue(progId, "Servo Speed", trkSpeed.Value.ToString());
                        profile.WriteValue(progId, "SoftClose", chkSoftClose.Checked.ToString());
                    }
                }
                catch (Exception ex)
                {
                    tl?.LogMessageCrLf("SetupDialog", $"Error saving profile: {ex.Message}");
                }
            }

            base.OnClosing(e);
        }

        // === Event Handlers ===

        private void TrkSpeed_ValueChanged(object sender, EventArgs e)
        {
            lblSpeedValue.Text = trkSpeed.Value.ToString();
        }

        private void BtnTestOpen_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                $"Um die Open-Position ({nudOpenPos.Value} µs) zu testen:\n\n" +
                "1. Verbinde dich mit dem Panel\n" +
                "2. Sende im Serial Monitor:\n" +
                $"   COMMAND:SETOPEN:{nudOpenPos.Value}\n" +
                "   COMMAND:OPEN\n\n" +
                "Die Test-Funktion bei verbundenem Treiber\n" +
                "wird in einer zukünftigen Version ergänzt.",
                "Test Open Position",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnTestClose_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                $"Um die Close-Position ({nudClosePos.Value} µs) zu testen:\n\n" +
                "1. Verbinde dich mit dem Panel\n" +
                "2. Sende im Serial Monitor:\n" +
                $"   COMMAND:SETCLOSE:{nudClosePos.Value}\n" +
                "   COMMAND:CLOSE\n\n" +
                "Die Test-Funktion bei verbundenem Treiber\n" +
                "wird in einer zukünftigen Version ergänzt.",
                "Test Close Position",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDefaults_Click(object sender, EventArgs e)
        {
            nudOpenPos.Value = 2500;
            nudClosePos.Value = 540;
            trkSpeed.Value = 5;
            chkSoftClose.Checked = false;
        }
    }
}
