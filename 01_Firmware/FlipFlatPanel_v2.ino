// ============================================================
// FlipFlat Panel - Arduino NANO Firmware v2.0
// ============================================================
// Credits:
//   Ursprüngliches Konzept, Schaltung und Hardware-Design:
//   Moritz Mayer / Dark Matters Discord
//   https://discord.gg/darkmatters
//
// Hardware (NICHT VERÄNDERN):
//   Pin 8  = Servo (manuelle Pulse, KEIN Timer1/Servo-Library)
//   Pin 9  = MOSFET -> EL-Folie (PWM via analogWrite/Timer1)
//   Serial = 57600 Baud
// ============================================================
// Änderungen gegenüber v1.0:
//   - Servo: Puls-Burst statt Einzelpuls (zuverlässige Bewegung)
//   - Alle Befehle liefern eine Antwort (kein ASCOM-Timeout)
//   - Non-blocking Serial-Read (kein delay() im Loop)
//   - Range-Check für Brightness (0-255)
//   - Konsistentes Antwortformat RESULT:BEFEHL:WERT
//   - Eingabepuffer mit Überlaufschutz
//   - Unbekannte Befehle werden mit ERROR beantwortet
// ============================================================

// --- Geräte-ID (muss zum ASCOM-Treiber passen) ---
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

// --- Antworten ---
constexpr auto RESULT_PING = "RESULT:PING:OK:";
constexpr auto RESULT_INFO = "RESULT:INFO:FlipFlatPanel Firmware v2.0";
constexpr auto ERROR_INVALID_COMMAND  = "ERROR:INVALID_COMMAND";

// ============================================================
// Pin-Belegung (HARDWARE - NICHT ÄNDERN)
// ============================================================
const int ServoPin = 8;
const int ledPin   = 9;

// ============================================================
// Servo-Konfiguration
// ============================================================
// Pulsbreiten in Mikrosekunden (aus Original übernommen)
const int SERVO_PULSE_OPEN  = 2500;   // Pulsbreite für "offen"
const int SERVO_PULSE_CLOSE = 540;    // Pulsbreite für "geschlossen"
const unsigned long SERVO_BURST_MS = 700;  // Dauer des Puls-Bursts

// ============================================================
// Zustandsvariablen
// ============================================================
int brightness = 0;

// Cover-Status: 0=unknown, 1=offen, 2=geschlossen, 3=moving
enum CoverState : uint8_t {
  COVER_UNKNOWN    = 0,
  COVER_OPEN       = 1,
  COVER_CLOSED     = 2,
  COVER_MOVING     = 3
};
CoverState coverState = COVER_UNKNOWN;

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

  bufferPos = 0;
}

// ============================================================
// Servo-Bewegung: Puls-Burst ohne Timer1
// ============================================================
// Sendet für SERVO_BURST_MS Millisekunden ein 50Hz-Signal.
// Timer1 bleibt frei -> analogWrite(pin9) funktioniert weiter.
// ============================================================
void moveServo(int pulseWidth_us) {
  unsigned long startTime = millis();

  while (millis() - startTime < SERVO_BURST_MS) {
    digitalWrite(ServoPin, HIGH);
    delayMicroseconds(pulseWidth_us);
    digitalWrite(ServoPin, LOW);

    // Restzeit des 20ms-Zyklus abwarten
    // (delayMicroseconds max. 16383 auf ATmega328, daher delay in ms)
    int waitMs = 20 - (pulseWidth_us / 1000);
    if (waitMs > 0) {
      delay(waitMs);
    }
  }

  // Sicherstellen, dass der Pin LOW ist
  digitalWrite(ServoPin, LOW);
}

// ============================================================
// Cover-Funktionen
// ============================================================
void openCover() {
  coverState = COVER_MOVING;
  moveServo(SERVO_PULSE_OPEN);
  coverState = COVER_OPEN;
  Serial.println("RESULT:OPEN:offen");
}

void closeCover() {
  coverState = COVER_MOVING;
  moveServo(SERVO_PULSE_CLOSE);
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
  // Argument nach "COMMAND:SETBRIGHTNESS:" extrahieren
  const char* arg = cmd + strlen(COMMAND_SETBRIGHTNESS);
  int value = atoi(arg);

  // Range-Check: 0-255
  if (value < 0)   value = 0;
  if (value > 255) value = 255;

  brightness = value;
  analogWrite(ledPin, brightness);

  Serial.print("RESULT:SETBRIGHTNESS:");
  Serial.println(brightness);
}

// ============================================================
// Befehlsverarbeitung
// ============================================================
void processCommand(const char* cmd) {
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

    // Zeilenende -> Befehl verarbeiten
    if (c == '\n' || c == '\r') {
      if (bufferPos > 0) {
        inputBuffer[bufferPos] = '\0';  // Nullterminierung
        processCommand(inputBuffer);
        bufferPos = 0;
      }
    }
    // Zeichen zum Puffer hinzufügen (mit Überlaufschutz)
    else if (bufferPos < INPUT_BUFFER_SIZE - 1) {
      inputBuffer[bufferPos++] = c;
    }
    // Pufferüberlauf -> Reset
    else {
      bufferPos = 0;
    }
  }
}
