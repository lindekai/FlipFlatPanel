// ============================================================
// FlipFlat Panel - ASCOM Setup Dialog
// ============================================================
// Ersetzt die generierte SetupDialogForm.cs im ASCOM-Projekt.
// Enthält COM-Port-Auswahl und Trace-Logging.
//
// Servo-Endpunkte, Geschwindigkeit und SoftClose liegen im EEPROM
// des Arduino und werden über die Controller-App eingestellt.
// ============================================================

using ASCOM.Utilities;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.FlipFlatPanel.CoverCalibrator
{
    [ComVisible(false)]
    public partial class SetupDialogForm : Form
    {
        TraceLogger tl;

        // Controls
        private ComboBox cmbComPort;
        private CheckBox chkTrace;
        private Button btnOK;
        private Button btnCancel;

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
            this.Size = new Size(420, 300);
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
            AddSeparator(y, "Servo");
            y += 30;

            // Hinweis: Servo-Einstellungen über die Controller-App
            Label lblServoHint = new Label();
            lblServoHint.Text = "Endpunkte, Geschwindigkeit und SoftClose sind im Arduino (EEPROM) gespeichert. " +
                                "Einstellen und testen über die FlipFlat Panel Controller-App im Direkt-Modus.";
            lblServoHint.Location = new Point(labelX, y);
            lblServoHint.Size = new Size(380, 48);
            lblServoHint.ForeColor = Color.FromArgb(136, 136, 153);
            this.Controls.Add(lblServoHint);
            y += 55;

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
            y += 40;

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
            chkTrace.Checked = tl.Enabled;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                // Werte in die Hardware-Klasse übernehmen (WriteProfile speichert sie danach)
                CoverCalibratorHardware.comPort = cmbComPort.SelectedItem?.ToString() ?? "COM1";
                tl.Enabled = chkTrace.Checked;
            }

            base.OnClosing(e);
        }
    }
}
