using System;
using System.Windows;
using System.Windows.Controls;

namespace FlipFlatPanel.Controller
{
    public partial class SettingsPanel : UserControl
    {
        // Callback-Funktion für serielle Kommunikation (wird von MainWindow gesetzt)
        public Func<string, System.Threading.Tasks.Task<string>> SendCommandAsync { get; set; }
        public Action<string> LogAction { get; set; }

        private bool isLoading = false;

        public SettingsPanel()
        {
            InitializeComponent();
        }

        // ============================================================
        // Öffentliche Methoden (aufgerufen von MainWindow)
        // ============================================================

        /// <summary>
        /// Aktiviert/deaktiviert alle Controls
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            sliderOpen.IsEnabled = enabled;
            sliderClose.IsEnabled = enabled;
            sliderSpeed.IsEnabled = enabled;
            chkSoftClose.IsEnabled = enabled;
            btnApply.IsEnabled = enabled;
            btnSaveEeprom.IsEnabled = enabled;
            btnDefaults.IsEnabled = enabled;
        }

        /// <summary>
        /// Liest die aktuellen Einstellungen vom Arduino
        /// </summary>
        public async System.Threading.Tasks.Task LoadFromDevice()
        {
            if (SendCommandAsync == null) return;

            isLoading = true;
            try
            {
                // Open Position
                string resp = await SendCommandAsync("COMMAND:GETOPEN");
                if (resp != null && resp.Contains("GETOPEN:"))
                {
                    string val = resp.Substring(resp.LastIndexOf(':') + 1).Trim();
                    if (int.TryParse(val, out int openVal))
                    {
                        openVal = Math.Max(500, Math.Min(2500, openVal));
                        sliderOpen.Value = openVal;
                    }
                }

                // Close Position
                resp = await SendCommandAsync("COMMAND:GETCLOSE");
                if (resp != null && resp.Contains("GETCLOSE:"))
                {
                    string val = resp.Substring(resp.LastIndexOf(':') + 1).Trim();
                    if (int.TryParse(val, out int closeVal))
                    {
                        closeVal = Math.Max(500, Math.Min(2500, closeVal));
                        sliderClose.Value = closeVal;
                    }
                }

                // Speed
                resp = await SendCommandAsync("COMMAND:GETSPEED");
                if (resp != null && resp.Contains("GETSPEED:"))
                {
                    string val = resp.Substring(resp.LastIndexOf(':') + 1).Trim();
                    if (int.TryParse(val, out int speedVal))
                    {
                        speedVal = Math.Max(1, Math.Min(10, speedVal));
                        sliderSpeed.Value = speedVal;
                    }
                }

                // SoftClose
                resp = await SendCommandAsync("COMMAND:GETSOFTCLOSE");
                if (resp != null && resp.Contains("GETSOFTCLOSE:"))
                {
                    string val = resp.Substring(resp.LastIndexOf(':') + 1).Trim();
                    chkSoftClose.IsChecked = val == "1";
                }

                Log("Einstellungen vom Arduino geladen.");
            }
            catch (Exception ex)
            {
                Log("Fehler beim Laden der Einstellungen: " + ex.Message);
            }
            finally
            {
                isLoading = false;
            }
        }

        // ============================================================
        // Slider Value Changed Events
        // ============================================================

        private void SliderOpen_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtOpenValue != null)
                txtOpenValue.Text = ((int)sliderOpen.Value).ToString();
        }

        private void SliderClose_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtCloseValue != null)
                txtCloseValue.Text = ((int)sliderClose.Value).ToString();
        }

        private void SliderSpeed_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtSpeedValue != null)
                txtSpeedValue.Text = ((int)sliderSpeed.Value).ToString();
        }

        private void SoftClose_Changed(object sender, RoutedEventArgs e)
        {
            // Nur für spätere Live-Updates
        }

        // ============================================================
        // Buttons
        // ============================================================

        /// <summary>
        /// Einstellungen an den Arduino senden (ohne EEPROM-Speicherung)
        /// </summary>
        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (SendCommandAsync == null) return;

            try
            {
                btnApply.IsEnabled = false;

                string resp;

                resp = await SendCommandAsync("COMMAND:SETOPEN:" + (int)sliderOpen.Value);
                Log("Open Position: " + (resp ?? "keine Antwort"));

                resp = await SendCommandAsync("COMMAND:SETCLOSE:" + (int)sliderClose.Value);
                Log("Close Position: " + (resp ?? "keine Antwort"));

                resp = await SendCommandAsync("COMMAND:SETSPEED:" + (int)sliderSpeed.Value);
                Log("Speed: " + (resp ?? "keine Antwort"));

                int softVal = chkSoftClose.IsChecked == true ? 1 : 0;
                resp = await SendCommandAsync("COMMAND:SETSOFTCLOSE:" + softVal);
                Log("SoftClose: " + (resp ?? "keine Antwort"));

                Log("\u2713 Einstellungen angewendet (noch nicht im EEPROM gespeichert).");
            }
            catch (Exception ex)
            {
                Log("\u2717 Fehler: " + ex.Message);
            }
            finally
            {
                btnApply.IsEnabled = true;
            }
        }

        /// <summary>
        /// Einstellungen an den Arduino senden UND im EEPROM speichern
        /// </summary>
        private async void SaveEeprom_Click(object sender, RoutedEventArgs e)
        {
            if (SendCommandAsync == null) return;

            try
            {
                btnSaveEeprom.IsEnabled = false;

                // Erst anwenden
                await SendCommandAsync("COMMAND:SETOPEN:" + (int)sliderOpen.Value);
                await SendCommandAsync("COMMAND:SETCLOSE:" + (int)sliderClose.Value);
                await SendCommandAsync("COMMAND:SETSPEED:" + (int)sliderSpeed.Value);
                int softVal = chkSoftClose.IsChecked == true ? 1 : 0;
                await SendCommandAsync("COMMAND:SETSOFTCLOSE:" + softVal);

                // Dann im EEPROM speichern
                string resp = await SendCommandAsync("COMMAND:SAVE");
                Log("EEPROM: " + (resp ?? "keine Antwort"));

                Log("\u2713 Einstellungen gespeichert (EEPROM). Bleiben nach Neustart erhalten.");
            }
            catch (Exception ex)
            {
                Log("\u2717 Fehler: " + ex.Message);
            }
            finally
            {
                btnSaveEeprom.IsEnabled = true;
            }
        }

        /// <summary>
        /// Standardwerte wiederherstellen
        /// </summary>
        private void Defaults_Click(object sender, RoutedEventArgs e)
        {
            isLoading = true;
            sliderOpen.Value = 2500;
            sliderClose.Value = 540;
            sliderSpeed.Value = 5;
            chkSoftClose.IsChecked = false;
            isLoading = false;

            Log("Standardwerte wiederhergestellt. Klicke 'Anwenden' oder 'Speichern' zum Übertragen.");
        }

        // ============================================================
        // Logging
        // ============================================================
        private void Log(string message)
        {
            LogAction?.Invoke(message);
        }
    }
}
