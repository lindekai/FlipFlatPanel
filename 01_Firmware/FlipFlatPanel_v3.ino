// ============================================================
// FlipFlat Panel - Arduino NANO Firmware v3.0
// ============================================================
// Credits:
//   Ursprüngliches Konzept, Schaltung und Hardware-Design:
//   Moritz Mayer / Dark Matters Discord
//   https://discord.gg/darkmatters
//
// Hardware (NICHT VERÄNDERN):
//   Pin 7  = Servo (manuelle Pulse, KEIN Timer1/Servo-Library)
//   Die PWM-fähigen Pins auf dem ATmega328P (Nano) sind nur: 3, 5, 6, 9, 10, 11.
//   Hier Pin 9  = MOSFET -> EL-Folie (PWM via analogWrite/Timer1)
//   Serial = 57600 Baud
// ============================================================
// Neu in v3.0:
//   - Einstellbare Servo-Endpunkte (Open/Close Position)
//   - Einstellbare Bewegungsgeschwindigkeit (1-10)
//   - SoftClose-Modus (langsam starten/stoppen)
//   - EEPROM-Speicherung aller Einstellungen
//   - Neue Befehle: SETOPEN, SETCLOSE, SETSPEED, SETSOFTCLOSE
// ============================================================

#include <EEPROM.h>
#include <math.h>

// --- Geräte-ID ---
constexpr auto DEVICE_GUID = "16c5e400-a3b1-11ed-87cd-0800200c9a66";

// --- Befehle (serielles Protokoll) ---
constexpr auto COMMAND_PING           = "COMMAND:PING";
constexpr auto COMMAND_INFO           = "COMMAND:INFO";
constexpr auto COMMAND_OPEN           = "COMMAND:OPEN";
constexpr auto COMMAND_CLOSE          = "COMMAND:CLOSE";
constexpr auto COMMAND_POSITION       = "COMMAND:POSITION";
constexpr auto COMMAND_BRIGHTNESS     = "COMMAND:BRIGHTNESS";
constexpr auto COMMAND_SETBRIGHTNESS  = "COMMAND:SETBRIGHTNESS:";
constexpr auto COMMAND_MAXBRIGHTNESS  = "COMMAND:MAXBRIGHTNESS";

// Neue Befehle v3.0
constexpr auto COMMAND_SETOPEN        = "COMMAND:SETOPEN:";
constexpr auto COMMAND_SETCLOSE       = "COMMAND:SETCLOSE:";
constexpr auto COMMAND_GETOPEN        = "COMMAND:GETOPEN";
constexpr auto COMMAND_GETCLOSE       = "COMMAND:GETCLOSE";
constexpr auto COMMAND_SETSPEED       = "COMMAND:SETSPEED:";
constexpr auto COMMAND_GETSPEED       = "COMMAND:GETSPEED";
constexpr auto COMMAND_SETSOFTCLOSE   = "COMMAND:SETSOFTCLOSE:";
constexpr auto COMMAND_GETSOFTCLOSE   = "COMMAND:GETSOFTCLOSE";
constexpr auto COMMAND_SAVE           = "COMMAND:SAVE";

// --- Antworten ---
constexpr auto RESULT_PING = "RESULT:PING:OK:";
constexpr auto RESULT_INFO = "RESULT:INFO:FlipFlatPanel Firmware v3.0";
constexpr auto ERROR_INVALID_COMMAND  = "ERROR:INVALID_COMMAND";

// ============================================================
// Pin-Belegung (HARDWARE - NICHT ÄNDERN)
// ============================================================
const int ServoPin = 7;
const int ledPin   = 9;

// ============================================================
// Servo-Grenzen (Hardware-Sicherheit)
// ============================================================
const int SERVO_PULSE_MIN = 500;    // Absolutes Minimum (µs)
const int SERVO_PULSE_MAX = 2500;   // Absolutes Maximum (µs)
const int SERVO_STEPS_PER_CYCLE = 1;  // Mikrosekunden pro Schritt (Auflösung)

// ============================================================
// EEPROM Layout
// ============================================================
const int EEPROM_MAGIC_ADDR     = 0;   // Magic Byte (1 Byte)
const int EEPROM_OPEN_ADDR      = 1;   // Open Position (2 Bytes)
const int EEPROM_CLOSE_ADDR     = 3;   // Close Position (2 Bytes)
const int EEPROM_SPEED_ADDR     = 5;   // Speed (1 Byte)
const int EEPROM_SOFTCLOSE_ADDR = 6;   // SoftClose (1 Byte)
const byte EEPROM_MAGIC_VALUE   = 0xA5; // Erkennung ob EEPROM initialisiert

// ============================================================
// Standard-Einstellungen
// ============================================================
const int DEFAULT_OPEN_PULSE    = 2500;
const int DEFAULT_CLOSE_PULSE   = 540;
const int DEFAULT_SPEED         = 5;     // 1=langsam, 10=schnell
const bool DEFAULT_SOFTCLOSE    = false;

// ============================================================
// Zustandsvariablen
// ============================================================
int brightness = 0;

enum CoverState : uint8_t {
  COVER_UNKNOWN    = 0,
  COVER_OPEN       = 1,
  COVER_CLOSED     = 2,
  COVER_MOVING     = 3
};
CoverState coverState = COVER_UNKNOWN;

// Aktuelle Position (in µs) – wird nach jeder Bewegung aktualisiert
int currentPulsePos = 0;

// Einstellungen (aus EEPROM oder Default)
int servoOpenPulse;
int servoClosePulse;
int servoSpeed;       // 1-10
bool softCloseEnabled;

// Serieller Eingabepuffer
const int INPUT_BUFFER_SIZE = 64;
char inputBuffer[INPUT_BUFFER_SIZE];
int bufferPos = 0;

// ============================================================
// Setup
// ============================================================
void setup() {
  Serial.begin(57600);
  while (!Serial) { ; }
  Serial.flush();

  pinMode(ServoPin, OUTPUT);
  digitalWrite(ServoPin, LOW);

  pinMode(ledPin, OUTPUT);
  analogWrite(ledPin, 0);

  // Einstellungen aus EEPROM laden
  loadSettings();

  bufferPos = 0;
}

// ============================================================
// EEPROM: Einstellungen laden / speichern
// ============================================================
void loadSettings() {
  if (EEPROM.read(EEPROM_MAGIC_ADDR) == EEPROM_MAGIC_VALUE) {
    // EEPROM ist initialisiert - Werte laden
    int openHi  = EEPROM.read(EEPROM_OPEN_ADDR);
    int openLo  = EEPROM.read(EEPROM_OPEN_ADDR + 1);
    servoOpenPulse = (openHi << 8) | openLo;

    int closeHi = EEPROM.read(EEPROM_CLOSE_ADDR);
    int closeLo = EEPROM.read(EEPROM_CLOSE_ADDR + 1);
    servoClosePulse = (closeHi << 8) | closeLo;

    servoSpeed = EEPROM.read(EEPROM_SPEED_ADDR);
    softCloseEnabled = EEPROM.read(EEPROM_SOFTCLOSE_ADDR) != 0;

    // Plausibilitäts-Check
    if (servoOpenPulse < SERVO_PULSE_MIN || servoOpenPulse > SERVO_PULSE_MAX)
      servoOpenPulse = DEFAULT_OPEN_PULSE;
    if (servoClosePulse < SERVO_PULSE_MIN || servoClosePulse > SERVO_PULSE_MAX)
      servoClosePulse = DEFAULT_CLOSE_PULSE;
    if (servoSpeed < 1 || servoSpeed > 10)
      servoSpeed = DEFAULT_SPEED;
  } else {
    // Erster Start: Standardwerte setzen
    servoOpenPulse = DEFAULT_OPEN_PULSE;
    servoClosePulse = DEFAULT_CLOSE_PULSE;
    servoSpeed = DEFAULT_SPEED;
    softCloseEnabled = DEFAULT_SOFTCLOSE;
    saveSettings(); // Schreibt auch Magic Byte
  }
}

void saveSettings() {
  EEPROM.update(EEPROM_MAGIC_ADDR, EEPROM_MAGIC_VALUE);

  EEPROM.update(EEPROM_OPEN_ADDR,     (servoOpenPulse >> 8) & 0xFF);
  EEPROM.update(EEPROM_OPEN_ADDR + 1,  servoOpenPulse & 0xFF);

  EEPROM.update(EEPROM_CLOSE_ADDR,     (servoClosePulse >> 8) & 0xFF);
  EEPROM.update(EEPROM_CLOSE_ADDR + 1,  servoClosePulse & 0xFF);

  EEPROM.update(EEPROM_SPEED_ADDR, (byte)servoSpeed);
  EEPROM.update(EEPROM_SOFTCLOSE_ADDR, softCloseEnabled ? 1 : 0);
}

// ============================================================
// Servo-Bewegung: Einen einzelnen Puls senden (50Hz)
// ============================================================
void sendServoPulse(int pulseWidth_us) {
  digitalWrite(ServoPin, HIGH);
  delayMicroseconds(pulseWidth_us);
  digitalWrite(ServoPin, LOW);

  // Restzeit des 20ms-Zyklus abwarten
  int waitMs = 20 - (pulseWidth_us / 1000);
  if (waitMs > 0) {
    delay(waitMs);
  }
}

// ============================================================
// Servo-Bewegung: Direkt (ohne Easing) - für Kompatibilität
// ============================================================
void moveServoDirect(int targetPulse) {
  // Geschwindigkeit: Speed 1 = 40ms pro Schritt, Speed 10 = 4ms pro Schritt
  int stepDelayMs = 44 - (servoSpeed * 4); // 40ms @ speed 1, 4ms @ speed 10
  int stepSize = 10; // µs pro Schritt

  int current = (currentPulsePos > 0) ? currentPulsePos : targetPulse;
  int direction = (targetPulse > current) ? 1 : -1;
  int steps = abs(targetPulse - current) / stepSize;

  if (steps == 0) {
    // Nur halten - Burst von Pulsen an Zielposition
    for (int i = 0; i < 25; i++) {
      sendServoPulse(targetPulse);
    }
  } else {
    for (int i = 0; i <= steps; i++) {
      int pos = current + (direction * i * stepSize);
      // Begrenzung
      if (direction > 0 && pos > targetPulse) pos = targetPulse;
      if (direction < 0 && pos < targetPulse) pos = targetPulse;

      sendServoPulse(pos);

      if (i < steps) {
        delay(stepDelayMs);
      }
    }
    // Einige Halte-Pulse am Ziel
    for (int i = 0; i < 10; i++) {
      sendServoPulse(targetPulse);
    }
  }

  currentPulsePos = targetPulse;
  digitalWrite(ServoPin, LOW);
}

// ============================================================
// Servo-Bewegung: SoftClose (Ease-In / Ease-Out)
// ============================================================
// Cosinus-Easing: langsam starten, schnell in der Mitte, langsam stoppen
// Formel: ease(t) = (1 - cos(t * PI)) / 2, wobei t = 0.0 bis 1.0
// ============================================================
void moveServoSoft(int targetPulse) {
  int current = (currentPulsePos > 0) ? currentPulsePos : targetPulse;
  int totalDelta = targetPulse - current;

  if (abs(totalDelta) < 10) {
    // Kaum Bewegung nötig - nur halten
    for (int i = 0; i < 25; i++) {
      sendServoPulse(targetPulse);
    }
    currentPulsePos = targetPulse;
    return;
  }

  // Anzahl der Schritte: Speed 1 = 100 Schritte, Speed 10 = 20 Schritte
  int numSteps = 120 - (servoSpeed * 10); // 110 @ speed 1, 20 @ speed 10
  if (numSteps < 15) numSteps = 15;

  // Basis-Schrittzeit: Speed 1 = 30ms, Speed 10 = 5ms
  int baseStepDelay = 35 - (servoSpeed * 3); // 32ms @ speed 1, 5ms @ speed 10
  if (baseStepDelay < 3) baseStepDelay = 3;

  for (int i = 0; i <= numSteps; i++) {
    // t geht von 0.0 bis 1.0
    float t = (float)i / (float)numSteps;

    // Cosinus-Easing: Position
    // ease(t) = (1 - cos(t * PI)) / 2
    float eased = (1.0 - cos(t * M_PI)) / 2.0;

    int pos = current + (int)(totalDelta * eased);

    // Begrenzung
    if (pos < SERVO_PULSE_MIN) pos = SERVO_PULSE_MIN;
    if (pos > SERVO_PULSE_MAX) pos = SERVO_PULSE_MAX;

    sendServoPulse(pos);

    if (i < numSteps) {
      // Variable Verzögerung: langsamer an den Enden
      // Derivative der Easing-Funktion: sin(t * PI) * PI / 2
      // Invertiert = langsam wo die Ableitung klein ist (Anfang/Ende)
      float speed_factor = sin(t * M_PI);
      if (speed_factor < 0.15) speed_factor = 0.15; // Minimum

      int stepDelay = (int)(baseStepDelay / speed_factor);
      if (stepDelay > baseStepDelay * 4) stepDelay = baseStepDelay * 4; // Cap
      if (stepDelay < 2) stepDelay = 2;

      delay(stepDelay);
    }
  }

  // Halte-Pulse am Ziel
  for (int i = 0; i < 10; i++) {
    sendServoPulse(targetPulse);
  }

  currentPulsePos = targetPulse;
  digitalWrite(ServoPin, LOW);
}

// ============================================================
// Servo-Bewegung: Dispatcher
// ============================================================
void moveServo(int targetPulse) {
  if (softCloseEnabled) {
    moveServoSoft(targetPulse);
  } else {
    moveServoDirect(targetPulse);
  }
}

// ============================================================
// Cover-Funktionen
// ============================================================
void openCover() {
  coverState = COVER_MOVING;
  moveServo(servoOpenPulse);
  coverState = COVER_OPEN;
  Serial.println("RESULT:OPEN:offen");
}

void closeCover() {
  coverState = COVER_MOVING;
  moveServo(servoClosePulse);
  coverState = COVER_CLOSED;
  Serial.println("RESULT:CLOSE:geschlossen");
}

// ============================================================
// Status-Abfragen
// ============================================================
const char* getCoverStateString() {
  switch (coverState) {
    case COVER_OPEN:    return "offen";
    case COVER_CLOSED:  return "geschlossen";
    case COVER_MOVING:  return "moving";
    default:            return "unknown";
  }
}

void handlePing() {
  Serial.print(RESULT_PING);
  Serial.println(DEVICE_GUID);
}

void sendFirmwareInfo() {
  Serial.println(RESULT_INFO);
}

void reportPosition() {
  Serial.print("RESULT:POSITION:");
  Serial.println(getCoverStateString());
}

void reportBrightness() {
  Serial.print("RESULT:BRIGHTNESS:");
  Serial.println(brightness);
}

void reportMaxBrightness() {
  Serial.print("RESULT:MAXBRIGHTNESS:");
  Serial.println(255);
}

// ============================================================
// Helligkeit setzen (mit Range-Check)
// ============================================================
void setBrightness(const char* cmd) {
  const char* arg = cmd + strlen(COMMAND_SETBRIGHTNESS);
  int value = atoi(arg);

  if (value < 0)   value = 0;
  if (value > 255) value = 255;

  brightness = value;
  analogWrite(ledPin, brightness);

  Serial.print("RESULT:SETBRIGHTNESS:");
  Serial.println(brightness);
}

// ============================================================
// Einstellungen setzen/abfragen (v3.0)
// ============================================================
void setOpenPosition(const char* cmd) {
  int value = atoi(cmd + strlen(COMMAND_SETOPEN));
  if (value < SERVO_PULSE_MIN) value = SERVO_PULSE_MIN;
  if (value > SERVO_PULSE_MAX) value = SERVO_PULSE_MAX;
  servoOpenPulse = value;
  Serial.print("RESULT:SETOPEN:");
  Serial.println(servoOpenPulse);
}

void setClosePosition(const char* cmd) {
  int value = atoi(cmd + strlen(COMMAND_SETCLOSE));
  if (value < SERVO_PULSE_MIN) value = SERVO_PULSE_MIN;
  if (value > SERVO_PULSE_MAX) value = SERVO_PULSE_MAX;
  servoClosePulse = value;
  Serial.print("RESULT:SETCLOSE:");
  Serial.println(servoClosePulse);
}

void setSpeed(const char* cmd) {
  int value = atoi(cmd + strlen(COMMAND_SETSPEED));
  if (value < 1)  value = 1;
  if (value > 10) value = 10;
  servoSpeed = value;
  Serial.print("RESULT:SETSPEED:");
  Serial.println(servoSpeed);
}

void setSoftClose(const char* cmd) {
  int value = atoi(cmd + strlen(COMMAND_SETSOFTCLOSE));
  softCloseEnabled = (value != 0);
  Serial.print("RESULT:SETSOFTCLOSE:");
  Serial.println(softCloseEnabled ? 1 : 0);
}

void handleSave() {
  saveSettings();
  Serial.println("RESULT:SAVE:OK");
}

// ============================================================
// Befehlsverarbeitung
// ============================================================
void processCommand(const char* cmd) {
  // Bestehende Befehle (v2.0 kompatibel)
  if (strcmp(cmd, COMMAND_PING) == 0) {
    handlePing();
  }
  else if (strcmp(cmd, COMMAND_INFO) == 0) {
    sendFirmwareInfo();
  }
  else if (strcmp(cmd, COMMAND_OPEN) == 0) {
    openCover();
  }
  else if (strcmp(cmd, COMMAND_CLOSE) == 0) {
    closeCover();
  }
  else if (strcmp(cmd, COMMAND_POSITION) == 0) {
    reportPosition();
  }
  else if (strcmp(cmd, COMMAND_BRIGHTNESS) == 0) {
    reportBrightness();
  }
  else if (strcmp(cmd, COMMAND_MAXBRIGHTNESS) == 0) {
    reportMaxBrightness();
  }
  else if (strncmp(cmd, COMMAND_SETBRIGHTNESS, strlen(COMMAND_SETBRIGHTNESS)) == 0) {
    setBrightness(cmd);
  }
  // Neue Befehle (v3.0)
  else if (strncmp(cmd, COMMAND_SETOPEN, strlen(COMMAND_SETOPEN)) == 0) {
    setOpenPosition(cmd);
  }
  else if (strncmp(cmd, COMMAND_SETCLOSE, strlen(COMMAND_SETCLOSE)) == 0) {
    setClosePosition(cmd);
  }
  else if (strcmp(cmd, COMMAND_GETOPEN) == 0) {
    Serial.print("RESULT:GETOPEN:");
    Serial.println(servoOpenPulse);
  }
  else if (strcmp(cmd, COMMAND_GETCLOSE) == 0) {
    Serial.print("RESULT:GETCLOSE:");
    Serial.println(servoClosePulse);
  }
  else if (strncmp(cmd, COMMAND_SETSPEED, strlen(COMMAND_SETSPEED)) == 0) {
    setSpeed(cmd);
  }
  else if (strcmp(cmd, COMMAND_GETSPEED) == 0) {
    Serial.print("RESULT:GETSPEED:");
    Serial.println(servoSpeed);
  }
  else if (strncmp(cmd, COMMAND_SETSOFTCLOSE, strlen(COMMAND_SETSOFTCLOSE)) == 0) {
    setSoftClose(cmd);
  }
  else if (strcmp(cmd, COMMAND_GETSOFTCLOSE) == 0) {
    Serial.print("RESULT:GETSOFTCLOSE:");
    Serial.println(softCloseEnabled ? 1 : 0);
  }
  else if (strcmp(cmd, COMMAND_SAVE) == 0) {
    handleSave();
  }
  else {
    Serial.println(ERROR_INVALID_COMMAND);
  }
}

// ============================================================
// Main Loop - Non-blocking Serial Read
// ============================================================
void loop() {
  while (Serial.available() > 0) {
    char c = Serial.read();

    if (c == '\n' || c == '\r') {
      if (bufferPos > 0) {
        inputBuffer[bufferPos] = '\0';
        processCommand(inputBuffer);
        bufferPos = 0;
      }
    }
    else if (bufferPos < INPUT_BUFFER_SIZE - 1) {
      inputBuffer[bufferPos++] = c;
    }
    else {
      bufferPos = 0;
    }
  }
}
