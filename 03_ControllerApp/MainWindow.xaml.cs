// ============================================================
// FlipFlat Panel Controller App
// ============================================================
// WPF-basierte Steuerungs-App für das FlipFlat Panel.
// Unterstützt direkten Serial-Modus und ASCOM-Treiber-Modus.
//
// Credits:
//   Ursprüngliches Konzept, Schaltung und Hardware-Design:
//   Moritz Mayer / Dark Matters Discord
//   https://discord.gg/darkmatters
//
// Lizenz: MIT
// ============================================================
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FlipFlatPanel.Controller
{
    public partial class MainWindow : Window
    {
        private const int BAUD_RATE = 57600;
        private const int SERIAL_TIMEOUT_MS = 5000;
        private const string DEVICE_GUID = "16c5e400-a3b1-11ed-87cd-0800200c9a66";
        private const string APP_VERSION = "1.2.1";
        private const string GITHUB_URL = "https://github.com/lindekai/FlipFlatPanel";

        private SerialPort serialPort = null;
        private bool isConnected = false;
        private bool isSerialMode = true;
        private dynamic ascomDevice = null;

        private DispatcherTimer brightnessTimer;
        private bool isBrightnessUserChange = true;
        private bool isBrightnessInputChange = false;
        private bool isSettingsLoading = false; // Verhindert Senden während dem Laden

        public MainWindow()
        {
            InitializeComponent();
            brightnessTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            brightnessTimer.Tick += BrightnessTimer_Tick;
            RefreshPortList();
            UpdateUI();
            Log("FlipFlat Panel Controller v" + APP_VERSION + " gestartet.");
        }

        // ============================================================
        // Menüleiste
        // ============================================================
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "FlipFlat Panel Controller - Hilfe\n\n" +
                "VERBINDUNG:\n" +
                "Wähle Modus und COM-Port, dann 'Verbinden'.\n" +
                "Der Arduino braucht ca. 2.5s für den Reset.\n\n" +
                "COVER-STEUERUNG:\n" +
                "ÖFFNEN/SCHLIESSEN steuert den Servo.\n\n" +
                "HELLIGKEIT:\n" +
                "Slider, Schnellwahl oder Direkteingabe (0-255 + Enter).\n\n" +
                "SERVO-EINSTELLUNGEN (Firmware v3.0):\n" +
                "Open/Close-Position: Endpunkte des Servos (500-2500 µs).\n" +
                "Geschwindigkeit: 1 (langsam) bis 10 (schnell).\n" +
                "SoftClose: Sanftes Beschleunigen und Bremsen.\n\n" +
                "'Übernehmen' sendet die Werte an den Arduino (temporär).\n" +
                "'Speichern' schreibt sie ins EEPROM (dauerhaft).\n" +
                "'Test Open/Close' testet die aktuellen Einstellungen.\n\n" +
                "TIPP: Zuerst mit 'Übernehmen' testen, dann 'Speichern'.\n\n"+
                "Clear Skies!",
                "Hilfe", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "FlipFlat Panel Controller\n" +
                "Version " + APP_VERSION + "\n\n" +
                "DIY Flat-Field Panel für Astrofotografie.\n\n" +
                "Komponenten:\n" +
                "  Arduino Nano (Firmware v3.0)\n" +
                "  Servo mit einstellbaren Endpunkten\n" +
                "  EL-Folie mit MOSFET-Dimmung (IRFZ44N)\n" +
                "  ASCOM CoverCalibrator Treiber (Platform 7)\n\n" +
                GITHUB_URL + "\n\nGitHub-Seite öffnen?",
                "Über", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
                try { Process.Start(new ProcessStartInfo(GITHUB_URL) { UseShellExecute = true }); } catch { }
        }

        // ============================================================
        // Modus & Ports
        // ============================================================
        private void Mode_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (isConnected) DisconnectDevice();
            isSerialMode = rbSerial.IsChecked == true;
            cmbPorts.IsEnabled = isSerialMode;
            UpdateUI();
        }

        private void RefreshPorts_Click(object sender, RoutedEventArgs e) { RefreshPortList(); }

        private void RefreshPortList()
        {
            string selected = cmbPorts.SelectedItem as string;
            cmbPorts.Items.Clear();
            var ports = SerialPort.GetPortNames().OrderBy(p => p).ToArray();
            foreach (var p in ports) cmbPorts.Items.Add(p);
            if (ports.Length > 0)
            {
                if (selected != null && ports.Contains(selected)) cmbPorts.SelectedItem = selected;
                else cmbPorts.SelectedIndex = 0;
            }
            Log(ports.Length + " COM-Port(s) gefunden.");
        }

        // ============================================================
        // Verbindung
        // ============================================================
        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected) DisconnectDevice();
            else await ConnectDevice();
        }

        private async Task ConnectDevice()
        {
            btnConnect.IsEnabled = false;
            btnConnect.Content = "Verbinde...";
            try
            {
                if (isSerialMode) await ConnectSerial();
                else await ConnectAscom();
                isConnected = true;
                Log("\u2713 Verbindung hergestellt!");

                // Servo-Einstellungen vom Arduino laden
                if (isSerialMode) await LoadServoSettings();
            }
            catch (Exception ex)
            {
                Log("\u2717 Verbindungsfehler: " + ex.Message);
                MessageBox.Show("Verbindung fehlgeschlagen:\n\n" + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { btnConnect.IsEnabled = true; UpdateUI(); }
        }

        private async Task ConnectSerial()
        {
            string port = cmbPorts.SelectedItem as string;
            if (string.IsNullOrEmpty(port)) throw new Exception("Kein COM-Port ausgewählt.");
            Log("Öffne " + port + " @ " + BAUD_RATE + " Baud...");
            serialPort = new SerialPort(port) { BaudRate = BAUD_RATE, Parity = Parity.None, DataBits = 8, StopBits = StopBits.One, Handshake = Handshake.None, ReadTimeout = SERIAL_TIMEOUT_MS, WriteTimeout = SERIAL_TIMEOUT_MS, NewLine = "\n", DtrEnable = true, RtsEnable = false };
            serialPort.Open(); serialPort.DiscardInBuffer(); serialPort.DiscardOutBuffer();
            Log("Warte auf Arduino-Reset (2.5s)...");
            await Task.Delay(2500);
            serialPort.DiscardInBuffer();
            string response = await SendSerialCommand("COMMAND:PING");
            if (response == null || !response.Contains(DEVICE_GUID))
            { serialPort.Close(); serialPort.Dispose(); serialPort = null; throw new Exception("Gerät nicht verifiziert.\nAntwort: " + (response ?? "keine")); }
            Log("Gerät verifiziert.");
            string info = await SendSerialCommand("COMMAND:INFO");
            txtFirmware.Text = "Firmware: " + (info ?? "\u2014");
            await QuerySerialState();
        }

        private async Task ConnectAscom()
        {
            Log("ASCOM Chooser...");
            await Task.Run(() =>
            {
                Type ct = Type.GetTypeFromProgID("ASCOM.Utilities.Chooser");
                if (ct == null) throw new Exception("ASCOM Platform nicht installiert.");
                dynamic chooser = Activator.CreateInstance(ct);
                chooser.DeviceType = "CoverCalibrator";
                string id = chooser.Choose("");
                if (string.IsNullOrEmpty(id)) throw new Exception("Kein Treiber ausgewählt.");
                Dispatcher.Invoke(() => Log("Treiber: " + id));
                Type dt = Type.GetTypeFromProgID(id);
                if (dt == null) throw new Exception("Treiber konnte nicht geladen werden.");
                ascomDevice = Activator.CreateInstance(dt);
                ascomDevice.Connected = true;
                Dispatcher.Invoke(() => { try { txtFirmware.Text = "ASCOM: " + ascomDevice.Name; } catch { txtFirmware.Text = "ASCOM: Verbunden"; } });
            });
            QueryAscomState();
        }

        private void DisconnectDevice()
        {
            try
            {
                if (isSerialMode)
                {
                    if (serialPort?.IsOpen == true)
                    { try { serialPort.WriteLine("COMMAND:SETBRIGHTNESS:0"); Thread.Sleep(200); } catch { } serialPort.Close(); serialPort.Dispose(); serialPort = null; }
                }
                else
                {
                    if (ascomDevice != null) { try { ascomDevice.CalibratorOff(); } catch { } try { ascomDevice.Connected = false; } catch { } ascomDevice = null; }
                }
                isConnected = false;
                Log("Verbindung getrennt.");
                txtFirmware.Text = "Firmware: \u2014";
            }
            catch (Exception ex) { Log("Fehler beim Trennen: " + ex.Message); }
            UpdateUI();
        }

        // ============================================================
        // Cover
        // ============================================================
        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            SetControlsEnabled(false);
            txtCoverState.Text = "Öffne...";
            ledCover.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0x47));
            try
            {
                if (isSerialMode) { Log("TX: COMMAND:OPEN"); var r = await SendSerialCommand("COMMAND:OPEN"); Log("RX: " + (r ?? "keine")); txtCoverState.Text = r?.Contains("offen") == true ? "Offen" : "Fehler"; ledCover.Fill = new SolidColorBrush(r?.Contains("offen") == true ? Color.FromRgb(0x6B, 0xCB, 0x77) : Color.FromRgb(0xFF, 0x6B, 0x6B)); }
                else { await Task.Run(() => ascomDevice.OpenCover()); txtCoverState.Text = "Offen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x6B, 0xCB, 0x77)); Log("Cover geöffnet."); }
            }
            catch (Exception ex) { Log("\u2717 " + ex.Message); txtCoverState.Text = "Fehler"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)); }
            finally { SetControlsEnabled(true); }
        }

        private async void Close_Click(object sender, RoutedEventArgs e)
        {
            SetControlsEnabled(false);
            txtCoverState.Text = "Schließe...";
            ledCover.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0x47));
            try
            {
                if (isSerialMode) { Log("TX: COMMAND:CLOSE"); var r = await SendSerialCommand("COMMAND:CLOSE"); Log("RX: " + (r ?? "keine")); txtCoverState.Text = r?.Contains("geschlossen") == true ? "Geschlossen" : "Fehler"; ledCover.Fill = new SolidColorBrush(r?.Contains("geschlossen") == true ? Color.FromRgb(0x5B, 0x9B, 0xD5) : Color.FromRgb(0xFF, 0x6B, 0x6B)); }
                else { await Task.Run(() => ascomDevice.CloseCover()); txtCoverState.Text = "Geschlossen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x5B, 0x9B, 0xD5)); Log("Cover geschlossen."); }
            }
            catch (Exception ex) { Log("\u2717 " + ex.Message); txtCoverState.Text = "Fehler"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)); }
            finally { SetControlsEnabled(true); }
        }

        // ============================================================
        // Helligkeit
        // ============================================================
        private void Brightness_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsLoaded) return;
            int v = (int)sliderBrightness.Value;
            if (!isBrightnessInputChange) txtBrightnessInput.Text = v.ToString();
            if (isBrightnessUserChange && isConnected) { brightnessTimer.Stop(); brightnessTimer.Start(); }
        }

        private async void BrightnessTimer_Tick(object sender, EventArgs e) { brightnessTimer.Stop(); await SetBrightness((int)sliderBrightness.Value); }

        private void BrightPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            { int v = int.Parse(btn.Tag.ToString()); isBrightnessUserChange = false; sliderBrightness.Value = v; txtBrightnessInput.Text = v.ToString(); isBrightnessUserChange = true; brightnessTimer.Stop(); _ = SetBrightness(v); }
        }

        private void BrightnessInput_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { ApplyBrightnessInput(); e.Handled = true; } }
        private void BrightnessInput_LostFocus(object sender, RoutedEventArgs e) { txtBrightnessInput.Text = ((int)sliderBrightness.Value).ToString(); }
        private void BrightnessSend_Click(object sender, RoutedEventArgs e) { ApplyBrightnessInput(); }

        private void ApplyBrightnessInput()
        {
            if (int.TryParse(txtBrightnessInput.Text.Trim(), out int v))
            { v = Math.Max(0, Math.Min(255, v)); isBrightnessInputChange = true; isBrightnessUserChange = false; sliderBrightness.Value = v; txtBrightnessInput.Text = v.ToString(); isBrightnessUserChange = true; isBrightnessInputChange = false; brightnessTimer.Stop(); _ = SetBrightness(v); }
            else { txtBrightnessInput.Text = ((int)sliderBrightness.Value).ToString(); Log("Ungültige Eingabe (0-255)."); }
        }

        private async Task SetBrightness(int value)
        {
            try
            {
                if (isSerialMode) { var r = await SendSerialCommand("COMMAND:SETBRIGHTNESS:" + value); Log("Helligkeit: " + (r ?? "keine Antwort")); }
                else { if (value > 0) await Task.Run(() => ascomDevice.CalibratorOn(value)); else await Task.Run(() => ascomDevice.CalibratorOff()); Log("Helligkeit: " + value); }
            }
            catch (Exception ex) { Log("\u2717 Helligkeit: " + ex.Message); }
        }

        // ============================================================
        // Servo-Einstellungen (v3.0)
        // ============================================================
        private async Task LoadServoSettings()
        {
            isSettingsLoading = true;
            try
            {
                Log("Lade Servo-Einstellungen...");

                string r = await SendSerialCommand("COMMAND:GETOPEN");
                if (r?.StartsWith("RESULT:GETOPEN:") == true && int.TryParse(r.Substring(15).Trim(), out int openVal))
                { sliderServoOpen.Value = openVal; txtServoOpen.Text = openVal.ToString(); Log("Open-Position: " + openVal + " µs"); }

                r = await SendSerialCommand("COMMAND:GETCLOSE");
                if (r?.StartsWith("RESULT:GETCLOSE:") == true && int.TryParse(r.Substring(16).Trim(), out int closeVal))
                { sliderServoClose.Value = closeVal; txtServoClose.Text = closeVal.ToString(); Log("Close-Position: " + closeVal + " µs"); }

                r = await SendSerialCommand("COMMAND:GETSPEED");
                if (r?.StartsWith("RESULT:GETSPEED:") == true && int.TryParse(r.Substring(16).Trim(), out int speedVal))
                { sliderSpeed.Value = speedVal; txtSpeed.Text = speedVal.ToString(); Log("Geschwindigkeit: " + speedVal); }

                r = await SendSerialCommand("COMMAND:GETSOFTCLOSE");
                if (r?.StartsWith("RESULT:GETSOFTCLOSE:") == true)
                { chkSoftClose.IsChecked = r.Substring(20).Trim() == "1"; Log("SoftClose: " + (chkSoftClose.IsChecked == true ? "Ein" : "Aus")); }

                Log("Servo-Einstellungen geladen.");
            }
            catch (Exception ex) { Log("Warnung: Servo-Einstellungen konnten nicht geladen werden: " + ex.Message); }
            finally { isSettingsLoading = false; }
        }

        private void ServoOpen_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        { if (IsLoaded) txtServoOpen.Text = ((int)sliderServoOpen.Value).ToString(); }

        private void ServoClose_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        { if (IsLoaded) txtServoClose.Text = ((int)sliderServoClose.Value).ToString(); }

        private void Speed_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        { if (IsLoaded) txtSpeed.Text = ((int)sliderSpeed.Value).ToString(); }

        private void SoftClose_Changed(object sender, RoutedEventArgs e) { /* Wird erst bei Übernehmen gesendet */ }

        private async void ServoApply_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;
            SetControlsEnabled(false);
            try
            {
                int openVal = (int)sliderServoOpen.Value;
                int closeVal = (int)sliderServoClose.Value;
                int speedVal = (int)sliderSpeed.Value;
                bool softClose = chkSoftClose.IsChecked == true;

                Log("Sende Servo-Einstellungen...");
                await SendSerialCommand("COMMAND:SETOPEN:" + openVal);
                await SendSerialCommand("COMMAND:SETCLOSE:" + closeVal);
                await SendSerialCommand("COMMAND:SETSPEED:" + speedVal);
                await SendSerialCommand("COMMAND:SETSOFTCLOSE:" + (softClose ? "1" : "0"));
                Log("\u2713 Einstellungen übernommen (temporär).");
            }
            catch (Exception ex) { Log("\u2717 Fehler: " + ex.Message); }
            finally { SetControlsEnabled(true); }
        }

        private async void ServoSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;
            try
            {
                // Erst übernehmen, dann speichern
                ServoApply_Click(sender, e);
                await Task.Delay(300);
                var r = await SendSerialCommand("COMMAND:SAVE");
                Log("\u2713 Einstellungen im EEPROM gespeichert: " + (r ?? ""));
            }
            catch (Exception ex) { Log("\u2717 Speichern fehlgeschlagen: " + ex.Message); }
        }

        private async void ServoTestOpen_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;
            SetControlsEnabled(false);
            txtCoverState.Text = "Test Open...";
            ledCover.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0x47));
            try
            {
                var r = await SendSerialCommand("COMMAND:OPEN");
                Log("Test Open: " + (r ?? "keine Antwort"));
                if (r?.Contains("offen") == true) { txtCoverState.Text = "Offen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x6B, 0xCB, 0x77)); }
            }
            catch (Exception ex) { Log("\u2717 " + ex.Message); }
            finally { SetControlsEnabled(true); }
        }

        private async void ServoTestClose_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected) return;
            SetControlsEnabled(false);
            txtCoverState.Text = "Test Close...";
            ledCover.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0xB3, 0x47));
            try
            {
                var r = await SendSerialCommand("COMMAND:CLOSE");
                Log("Test Close: " + (r ?? "keine Antwort"));
                if (r?.Contains("geschlossen") == true) { txtCoverState.Text = "Geschlossen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x5B, 0x9B, 0xD5)); }
            }
            catch (Exception ex) { Log("\u2717 " + ex.Message); }
            finally { SetControlsEnabled(true); }
        }

        // ============================================================
        // Ping
        // ============================================================
        private async void Ping_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isSerialMode) { var r = await SendSerialCommand("COMMAND:PING"); Log("PING: " + (r ?? "keine Antwort")); }
                else { bool c = await Task.Run(() => (bool)ascomDevice.Connected); Log("ASCOM Connected: " + c); }
            }
            catch (Exception ex) { Log("\u2717 " + ex.Message); }
        }

        // ============================================================
        // Serial
        // ============================================================
        private async Task<string> SendSerialCommand(string command)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (serialPort == null || !serialPort.IsOpen) return null;
                    serialPort.DiscardInBuffer();
                    serialPort.WriteLine(command);
                    return serialPort.ReadLine().Trim();
                }
                catch (TimeoutException) { Dispatcher.Invoke(() => Log("\u23F1 Timeout: " + command)); return null; }
                catch (Exception ex) { Dispatcher.Invoke(() => Log("\u2717 Serial: " + ex.Message)); return null; }
            });
        }

        // ============================================================
        // State Queries
        // ============================================================
        private async Task QuerySerialState()
        {
            try
            {
                string pos = await SendSerialCommand("COMMAND:POSITION");
                if (pos?.Contains("offen") == true) { txtCoverState.Text = "Offen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x6B, 0xCB, 0x77)); }
                else if (pos?.Contains("geschlossen") == true) { txtCoverState.Text = "Geschlossen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x5B, 0x9B, 0xD5)); }
                else { txtCoverState.Text = "Unbekannt"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x99)); }

                string bright = await SendSerialCommand("COMMAND:BRIGHTNESS");
                if (bright != null)
                {
                    string v = bright.Contains(":") ? bright.Substring(bright.LastIndexOf(':') + 1).Trim() : "0";
                    if (int.TryParse(v, out int val)) { isBrightnessUserChange = false; sliderBrightness.Value = val; txtBrightnessInput.Text = val.ToString(); isBrightnessUserChange = true; }
                }
            }
            catch (Exception ex) { Log("Zustandsabfrage: " + ex.Message); }
        }

        private void QueryAscomState()
        {
            try
            {
                int cs = (int)ascomDevice.CoverState;
                switch (cs)
                {
                    case 1: txtCoverState.Text = "Geschlossen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x5B, 0x9B, 0xD5)); break;
                    case 3: txtCoverState.Text = "Offen"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x6B, 0xCB, 0x77)); break;
                    default: txtCoverState.Text = "Unbekannt"; ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x99)); break;
                }
                int b = (int)ascomDevice.Brightness;
                isBrightnessUserChange = false; sliderBrightness.Value = b; txtBrightnessInput.Text = b.ToString(); isBrightnessUserChange = true;
            }
            catch (Exception ex) { Log("ASCOM: " + ex.Message); }
        }

        // ============================================================
        // UI
        // ============================================================
        private void UpdateUI()
        {
            if (isConnected)
            {
                ledConnection.Fill = new SolidColorBrush(Color.FromRgb(0x6B, 0xCB, 0x77));
                txtConnectionStatus.Text = "Verbunden";
                txtConnectionStatus.Foreground = FindResource("AccentGreen") as Brush;
                btnConnect.Content = "Trennen";
                btnConnect.Background = FindResource("AccentRed") as Brush;
            }
            else
            {
                ledConnection.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B));
                txtConnectionStatus.Text = "Getrennt";
                txtConnectionStatus.Foreground = FindResource("TextDim") as Brush;
                btnConnect.Content = "Verbinden";
                btnConnect.Background = FindResource("AccentBlue") as Brush;
                ledCover.Fill = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                txtCoverState.Text = "\u2014";
                isBrightnessUserChange = false; sliderBrightness.Value = 0; txtBrightnessInput.Text = "0"; isBrightnessUserChange = true;
            }
            SetControlsEnabled(isConnected);
        }

        private void SetControlsEnabled(bool enabled)
        {
            bool s = enabled && isConnected;
            btnOpen.IsEnabled = s; btnClose.IsEnabled = s;
            sliderBrightness.IsEnabled = s; txtBrightnessInput.IsEnabled = s; btnBrightnessSend.IsEnabled = s;
            btnBright0.IsEnabled = s; btnBright25.IsEnabled = s; btnBright50.IsEnabled = s; btnBright75.IsEnabled = s; btnBright100.IsEnabled = s;
            btnPing.IsEnabled = s;
            // Servo-Einstellungen
            sliderServoOpen.IsEnabled = s; sliderServoClose.IsEnabled = s; sliderSpeed.IsEnabled = s; chkSoftClose.IsEnabled = s;
            btnServoApply.IsEnabled = s; btnServoSave.IsEnabled = s; btnServoTestOpen.IsEnabled = s; btnServoTestClose.IsEnabled = s;
            // Nicht-verbunden
            cmbPorts.IsEnabled = !isConnected && isSerialMode;
            rbSerial.IsEnabled = !isConnected; rbAscom.IsEnabled = !isConnected;
        }

        // ============================================================
        // Logging
        // ============================================================
        private void Log(string message)
        {
            if (!Dispatcher.CheckAccess()) { Dispatcher.Invoke(() => Log(message)); return; }
            txtLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message + "\n");
            txtLog.ScrollToEnd();
            if (txtLog.LineCount > 500) { int i = txtLog.Text.IndexOf('\n'); if (i > 0) txtLog.Text = txtLog.Text.Substring(i + 1); }
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e) { txtLog.Clear(); Log("Protokoll gelöscht."); }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) { if (isConnected) DisconnectDevice(); base.OnClosing(e); }
    }
}
