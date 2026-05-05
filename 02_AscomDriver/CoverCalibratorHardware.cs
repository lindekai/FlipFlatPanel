// ============================================================
// ASCOM CoverCalibrator hardware class for FlipFlat Panel
// ============================================================
// Description:  FlipFlat Panel - Arduino Nano mit Servo und EL-Folie
// Implements:   ASCOM CoverCalibrator interface version: 2 (Platform 7)
// Firmware:     v3.0 (einstellbare Endpunkte, Speed, SoftClose)
//
// Credits:
//   Ursprüngliches Konzept, Schaltung und Hardware-Design:
//   Moritz Mayer / Dark Matters Discord
//   https://discord.gg/darkmatters
//
// Author:     Kai Linde
// GitHub:     https://github.com/lindekai/
// Lizenz: MIT
// ============================================================

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Astrometry.NOVAS;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASCOM.FlipFlatPanel.CoverCalibrator
{
    [HardwareClass()]
    internal static class CoverCalibratorHardware
    {
        internal const string comPortProfileName = "COM Port";
        internal const string comPortDefault = "COM1";
        internal const string traceStateProfileName = "Trace Level";
        internal const string traceStateDefault = "true";

        private static string DriverProgId = "";
        private static string DriverDescription = "";
        internal static string comPort;
        private static bool connectedState;
        private static bool runOnce = false;
        internal static Util utilities;
        internal static AstroUtils astroUtilities;
        internal static TraceLogger tl;
        private static List<Guid> uniqueIds = new List<Guid>();

        // Serial & Protocol
        private const string DEVICE_GUID = "16c5e400-a3b1-11ed-87cd-0800200c9a66";
        private const int BAUD_RATE = 57600;
        private const int SERIAL_TIMEOUT_MS = 5000;
        private const int MOVE_TIMEOUT_MS = 30000;  // 30s für Servo-Bewegungen (SoftClose + langsame Geschwindigkeit)
        private const int CONNECT_RETRY_COUNT = 3;
        private const int CONNECT_RETRY_DELAY_MS = 500;
        private const int MAX_BRIGHTNESS_VALUE = 255;

        private static SerialPort serialPort = null;
        private static readonly object serialLock = new object();
        private static CoverStatus currentCoverState = CoverStatus.Unknown;
        private static CalibratorStatus currentCalibratorState = CalibratorStatus.Off;
        private static int currentBrightness = 0;

        // Servo Settings (v3.0)
        private static int servoOpenPulse = 2500;
        private static int servoClosePulse = 540;
        private static int servoSpeed = 5;
        private static bool softCloseEnabled = false;

        static CoverCalibratorHardware()
        {
            try
            {
                tl = new TraceLogger("", "FlipFlatPanel.Hardware");
                DriverProgId = CoverCalibrator.DriverProgId;
                ReadProfile();
                LogMessage("CoverCalibratorHardware", $"Static initialiser completed.");
            }
            catch (Exception ex)
            {
                try { LogMessage("CoverCalibratorHardware", $"Initialisation exception: {ex}"); } catch { }
                MessageBox.Show($"{ex.Message}\r\n{ex}", $"Exception creating {CoverCalibrator.DriverProgId}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        internal static void InitialiseHardware()
        {
            LogMessage("InitialiseHardware", $"Start.");
            if (runOnce == false)
            {
                DriverDescription = CoverCalibrator.DriverDescription;
                connectedState = false;
                utilities = new Util();
                astroUtilities = new AstroUtils();
                currentCoverState = CoverStatus.Unknown;
                currentCalibratorState = CalibratorStatus.Off;
                currentBrightness = 0;
                runOnce = true;
                LogMessage("InitialiseHardware", $"One-off initialisation complete.");
            }
        }

        #region Common properties and methods

        public static void SetupDialog()
        {
            if (IsConnected) { MessageBox.Show("Already connected, just press OK"); return; }
            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                if (F.ShowDialog() == DialogResult.OK) WriteProfile();
            }
        }

        public static ArrayList SupportedActions
        {
            get { return new ArrayList(); }
        }

        public static string Action(string actionName, string actionParameters)
        {
            throw new ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public static void CommandBlind(string command, bool raw) { CheckConnected("CommandBlind"); SendCommand(command); }
        public static bool CommandBool(string command, bool raw) { CheckConnected("CommandBool"); throw new MethodNotImplementedException("CommandBool"); }
        public static string CommandString(string command, bool raw) { CheckConnected("CommandString"); return SendCommand(command); }

        public static void Dispose()
        {
            try { CloseSerialPort(); } catch { }
            try { tl.Enabled = false; tl.Dispose(); tl = null; } catch { }
            try { utilities.Dispose(); utilities = null; } catch { }
            try { astroUtilities.Dispose(); astroUtilities = null; } catch { }
        }

        public static void SetConnected(Guid uniqueId, bool newState)
        {
            if (newState)
            {
                if (!uniqueIds.Contains(uniqueId))
                {
                    if (uniqueIds.Count == 0)
                    {
                        LogMessage("SetConnected", $"Connecting to {comPort}...");
                        try
                        {
                            OpenSerialPort();
                            Thread.Sleep(2500);
                            lock (serialLock) { if (serialPort?.IsOpen == true) serialPort.DiscardInBuffer(); }

                            bool verified = false;
                            for (int i = 0; i < CONNECT_RETRY_COUNT; i++)
                            {
                                try
                                {
                                    string r = SendCommand("COMMAND:PING");
                                    if (r?.Contains(DEVICE_GUID) == true) { verified = true; break; }
                                }
                                catch (TimeoutException) { }
                                Thread.Sleep(CONNECT_RETRY_DELAY_MS);
                            }
                            if (!verified) { CloseSerialPort(); throw new DriverException($"Device not verified on {comPort}."); }

                            QueryDeviceState();
                            QueryServoSettings();
                            connectedState = true;
                            LogMessage("SetConnected", "Connected.");
                        }
                        catch (Exception)
                        {
                            CloseSerialPort();
                            connectedState = false;
                            throw;
                        }
                    }
                    uniqueIds.Add(uniqueId);
                }
            }
            else
            {
                if (uniqueIds.Contains(uniqueId))
                {
                    uniqueIds.Remove(uniqueId);
                    if (uniqueIds.Count == 0)
                    {
                        try { if (connectedState && currentBrightness > 0) SendCommand("COMMAND:SETBRIGHTNESS:0"); } catch { }
                        CloseSerialPort();
                        connectedState = false;
                        currentCoverState = CoverStatus.Unknown;
                        currentCalibratorState = CalibratorStatus.Off;
                        currentBrightness = 0;
                    }
                }
            }
        }

        public static string Description { get { return DriverDescription; } }

        public static string DriverInfo
        {
            get
            {
                Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return $"FlipFlat Panel ASCOM CoverCalibrator Driver. Version: {v.Major}.{v.Minor}";
            }
        }

        public static string DriverVersion
        {
            get { Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; return $"{v.Major}.{v.Minor}"; }
        }

        public static short InterfaceVersion { get { return 2; } }
        public static string Name { get { return "FlipFlat Panel"; } }

        #endregion

        #region ICoverCalibrator Implementation

        internal static CoverStatus CoverState { get { return currentCoverState; } }
        internal static bool CoverMoving { get { return currentCoverState == CoverStatus.Moving; } }

        internal static void OpenCover()
        {
            CheckConnected("OpenCover");
            currentCoverState = CoverStatus.Moving;
            try
            {
                // SendCommandLong: 30s Timeout für langsame Bewegungen + SoftClose
                string r = SendCommandLong("COMMAND:OPEN");
                currentCoverState = r?.Contains("offen") == true ? CoverStatus.Open : CoverStatus.Error;
                if (currentCoverState == CoverStatus.Error) throw new DriverException("OpenCover failed.");
            }
            catch (TimeoutException) { currentCoverState = CoverStatus.Error; throw new DriverException("OpenCover timeout."); }
        }

        internal static void CloseCover()
        {
            CheckConnected("CloseCover");
            currentCoverState = CoverStatus.Moving;
            try
            {
                // SendCommandLong: 30s Timeout für langsame Bewegungen + SoftClose
                string r = SendCommandLong("COMMAND:CLOSE");
                currentCoverState = r?.Contains("geschlossen") == true ? CoverStatus.Closed : CoverStatus.Error;
                if (currentCoverState == CoverStatus.Error) throw new DriverException("CloseCover failed.");
            }
            catch (TimeoutException) { currentCoverState = CoverStatus.Error; throw new DriverException("CloseCover timeout."); }
        }

        internal static void HaltCover() { LogMessage("HaltCover", "Halt requested."); }

        internal static CalibratorStatus CalibratorState { get { return currentCalibratorState; } }
        internal static bool CalibratorChanging { get { return false; } }
        internal static int Brightness { get { return currentBrightness; } }
        internal static int MaxBrightness { get { return MAX_BRIGHTNESS_VALUE; } }

        internal static void CalibratorOn(int Brightness)
        {
            CheckConnected("CalibratorOn");
            if (Brightness < 0 || Brightness > MAX_BRIGHTNESS_VALUE)
                throw new InvalidValueException("CalibratorOn", Brightness.ToString(), $"0 - {MAX_BRIGHTNESS_VALUE}");
            try
            {
                string r = SendCommand("COMMAND:SETBRIGHTNESS:" + Brightness);
                if (r?.StartsWith("RESULT:SETBRIGHTNESS:") == true)
                    currentBrightness = int.TryParse(r.Substring(21).Trim(), out int v) ? v : Brightness;
                else currentBrightness = Brightness;
                currentCalibratorState = currentBrightness > 0 ? CalibratorStatus.Ready : CalibratorStatus.Off;
            }
            catch (TimeoutException) { currentCalibratorState = CalibratorStatus.Error; throw new DriverException("CalibratorOn timeout."); }
        }

        internal static void CalibratorOff()
        {
            CheckConnected("CalibratorOff");
            try { SendCommand("COMMAND:SETBRIGHTNESS:0"); currentBrightness = 0; currentCalibratorState = CalibratorStatus.Off; }
            catch (TimeoutException) { currentCalibratorState = CalibratorStatus.Error; throw new DriverException("CalibratorOff timeout."); }
        }

        #endregion

        #region Servo Settings (v3.0)

        internal static int ServoOpenPulse { get { return servoOpenPulse; } }
        internal static int ServoClosePulse { get { return servoClosePulse; } }
        internal static int ServoSpeed { get { return servoSpeed; } }
        internal static bool SoftCloseEnabled { get { return softCloseEnabled; } }

        internal static void QueryServoSettings()
        {
            try
            {
                string r;
                r = SendCommand("COMMAND:GETOPEN");
                if (r?.StartsWith("RESULT:GETOPEN:") == true) int.TryParse(r.Substring(15).Trim(), out servoOpenPulse);
                r = SendCommand("COMMAND:GETCLOSE");
                if (r?.StartsWith("RESULT:GETCLOSE:") == true) int.TryParse(r.Substring(16).Trim(), out servoClosePulse);
                r = SendCommand("COMMAND:GETSPEED");
                if (r?.StartsWith("RESULT:GETSPEED:") == true) int.TryParse(r.Substring(16).Trim(), out servoSpeed);
                r = SendCommand("COMMAND:GETSOFTCLOSE");
                if (r?.StartsWith("RESULT:GETSOFTCLOSE:") == true) softCloseEnabled = r.Substring(20).Trim() == "1";
                LogMessage("QueryServoSettings", $"Open:{servoOpenPulse} Close:{servoClosePulse} Speed:{servoSpeed} Soft:{softCloseEnabled}");
            }
            catch (Exception ex) { LogMessage("QueryServoSettings", $"Warning: {ex.Message}"); }
        }

        internal static void SetServoOpenPulse(int value) { CheckConnected("SetServoOpenPulse"); SendCommand("COMMAND:SETOPEN:" + value); servoOpenPulse = value; }
        internal static void SetServoClosePulse(int value) { CheckConnected("SetServoClosePulse"); SendCommand("COMMAND:SETCLOSE:" + value); servoClosePulse = value; }
        internal static void SetServoSpeed(int value) { CheckConnected("SetServoSpeed"); SendCommand("COMMAND:SETSPEED:" + value); servoSpeed = value; }
        internal static void SetSoftClose(bool enabled) { CheckConnected("SetSoftClose"); SendCommand("COMMAND:SETSOFTCLOSE:" + (enabled ? "1" : "0")); softCloseEnabled = enabled; }
        internal static void SaveServoSettings() { CheckConnected("SaveServoSettings"); SendCommand("COMMAND:SAVE"); }
        internal static string[] GetAvailablePorts() { return SerialPort.GetPortNames(); }

        #endregion

        #region Private

        private static bool IsConnected { get { return connectedState; } }
        private static void CheckConnected(string msg) { if (!IsConnected) throw new NotConnectedException(msg); }

        private static void OpenSerialPort()
        {
            lock (serialLock)
            {
                if (serialPort?.IsOpen == true) { serialPort.Close(); serialPort.Dispose(); }
                serialPort = new SerialPort(comPort) { BaudRate = BAUD_RATE, Parity = Parity.None, DataBits = 8, StopBits = StopBits.One, Handshake = Handshake.None, ReadTimeout = SERIAL_TIMEOUT_MS, WriteTimeout = SERIAL_TIMEOUT_MS, NewLine = "\n", DtrEnable = true, RtsEnable = false };
                serialPort.Open(); serialPort.DiscardInBuffer(); serialPort.DiscardOutBuffer();
            }
        }

        private static void CloseSerialPort()
        {
            lock (serialLock)
            {
                if (serialPort != null)
                {
                    try { if (serialPort.IsOpen) { serialPort.DiscardInBuffer(); serialPort.DiscardOutBuffer(); serialPort.Close(); } serialPort.Dispose(); } catch { }
                    serialPort = null;
                }
            }
        }

        /// <summary>
        /// Standard-Befehl senden (5s Timeout) – für PING, BRIGHTNESS, Settings etc.
        /// </summary>
        private static string SendCommand(string command)
        {
            lock (serialLock)
            {
                if (serialPort == null || !serialPort.IsOpen) throw new NotConnectedException("Serial port not available.");
                serialPort.DiscardInBuffer();
                LogMessage("SendCommand", $"TX: {command}");
                serialPort.WriteLine(command);
                string response = serialPort.ReadLine().Trim();
                LogMessage("SendCommand", $"RX: {response}");
                return response;
            }
        }

        /// <summary>
        /// Langer Befehl senden (30s Timeout) – für OPEN/CLOSE bei langsamer
        /// Geschwindigkeit und SoftClose, wo die Servo-Bewegung 10-20s dauern kann.
        /// </summary>
        private static string SendCommandLong(string command)
        {
            lock (serialLock)
            {
                if (serialPort == null || !serialPort.IsOpen) throw new NotConnectedException("Serial port not available.");
                int oldTimeout = serialPort.ReadTimeout;
                serialPort.ReadTimeout = MOVE_TIMEOUT_MS;
                try
                {
                    serialPort.DiscardInBuffer();
                    LogMessage("SendCommandLong", $"TX: {command} (timeout: {MOVE_TIMEOUT_MS}ms)");
                    serialPort.WriteLine(command);
                    string response = serialPort.ReadLine().Trim();
                    LogMessage("SendCommandLong", $"RX: {response}");
                    return response;
                }
                finally
                {
                    serialPort.ReadTimeout = oldTimeout;
                }
            }
        }

        private static void QueryDeviceState()
        {
            try
            {
                string r = SendCommand("COMMAND:POSITION");
                if (r?.Contains("offen") == true) currentCoverState = CoverStatus.Open;
                else if (r?.Contains("geschlossen") == true) currentCoverState = CoverStatus.Closed;
                else currentCoverState = CoverStatus.Unknown;

                r = SendCommand("COMMAND:BRIGHTNESS");
                if (r?.Contains("BRIGHTNESS:") == true)
                {
                    if (int.TryParse(r.Substring(r.LastIndexOf(':') + 1).Trim(), out int val))
                    { currentBrightness = val; currentCalibratorState = val > 0 ? CalibratorStatus.Ready : CalibratorStatus.Off; }
                }
            }
            catch (Exception ex) { LogMessage("QueryDeviceState", $"Warning: {ex.Message}"); currentCoverState = CoverStatus.Unknown; currentBrightness = 0; }
        }

        internal static void ReadProfile()
        {
            using (Profile p = new Profile()) { p.DeviceType = "CoverCalibrator"; tl.Enabled = Convert.ToBoolean(p.GetValue(DriverProgId, traceStateProfileName, "", traceStateDefault)); comPort = p.GetValue(DriverProgId, comPortProfileName, "", comPortDefault); }
        }

        internal static void WriteProfile()
        {
            using (Profile p = new Profile()) { p.DeviceType = "CoverCalibrator"; p.WriteValue(DriverProgId, traceStateProfileName, tl.Enabled.ToString()); p.WriteValue(DriverProgId, comPortProfileName, comPort); }
        }

        internal static void LogMessage(string id, string msg) { tl.LogMessageCrLf(id, msg); }
        internal static void LogMessage(string id, string msg, params object[] args) { LogMessage(id, string.Format(msg, args)); }

        #endregion
    }
}
