# FlipFlat Panel

Ein DIY Flat-Field Panel für die Astrofotografie mit Arduino Nano, Servo-Klappe und EL-Folie. Gesteuert über ASCOM CoverCalibrator (Platform 7) für den Einsatz in N.I.N.A. und anderer Astro-Software.

![Status](https://img.shields.io/badge/Status-In%20Entwicklung-yellow)
![ASCOM](https://img.shields.io/badge/ASCOM-Platform%207-blue)
![Arduino](https://img.shields.io/badge/Arduino-Nano-teal)
![License](https://img.shields.io/badge/License-MIT-green)

## Übersicht

Das FlipFlat Panel kombiniert einen motorisierten Deckel (Flip) mit einem dimmbaren Flat-Field-Panel (Flat) in einem Gerät. Es kann über N.I.N.A., ASCOM-kompatible Software oder die mitgelieferte Desktop-App gesteuert werden.

### Funktionen

- **Cover-Steuerung**: Servo öffnet und schließt den Deckel automatisch
- **Helligkeit**: EL-Folie stufenlos dimmbar (0-255) über MOSFET-PWM
- **ASCOM CoverCalibrator**: Vollständige ICoverCalibratorV2-Implementierung für Platform 7
- **Desktop-App**: Eigenständige Steuerungs-App mit Serial- und ASCOM-Modus
- **N.I.N.A. kompatibel**: Automatisierte Flat-Frame-Aufnahme

## Hardware

### Komponenten

| Bauteil | Beschreibung |
|---------|-------------|
| Arduino Nano (Rev3) | Mikrocontroller, USB-Seriell |
| Servo | Flip-Mechanismus für den Deckel |
| EL-Folie + Inverter | Gleichmäßige Ausleuchtung als Flat-Field-Quelle |
| IRFZ44N MOSFET | Schaltet/dimmt den EL-Inverter per PWM |
| 10kΩ Widerstand | Pull-Down am MOSFET Gate |
| 12V DC Netzteil | Stromversorgung für den EL-Inverter |

### Pin-Belegung

| Arduino Pin | Funktion | Beschreibung |
|-------------|----------|-------------|
| Pin 8 | Servo | Manuelle Pulse (kein Timer1, um PWM auf Pin 9 zu erhalten) |
| Pin 9 | MOSFET Gate | PWM-Ausgang für EL-Folie Helligkeit |
| USB | Serial | 57600 Baud, Steuerungsprotokoll |

### Schaltplan

Der IRFZ44N MOSFET schaltet die Masse-Seite des EL-Inverters. Ein 10kΩ Pull-Down-Widerstand am Gate sorgt dafür, dass die EL-Folie beim Arduino-Start dunkel bleibt.

```
Arduino Pin 9 ──── Gate
                    │
                  ┌─┴─┐
            10kΩ  │   │ IRFZ44N
                  └─┬─┘
                    │
                   GND

12V DC ──── EL-Inverter ──── Drain
```
![Schaltplan(]https://github.com/lindekai/FlipFlatPanel/blob/main/docs/FlatPanel_KL_V.1.4_Schaltplan.jpg)

## Software-Architektur

Das Projekt besteht aus drei Komponenten:

```
FlipFlatPanel/
├── 01_Firmware/              Arduino Nano Firmware (v2.0)
│   └── FlipFlatPanel_v2.ino
├── 02_AscomDriver/           ASCOM CoverCalibrator Treiber
│   └── CoverCalibratorHardware.cs
├── 03_ControllerApp/         Desktop-Steuerungs-App (WPF)
│   ├── FlipFlatPanel.Controller.csproj
│   ├── App.xaml / App.xaml.cs
│   └── MainWindow.xaml / MainWindow.xaml.cs
└── docs/                     Dokumentation
```

## Installation

### 1. Arduino Firmware flashen

1. Öffne `01_Firmware/FlipFlatPanel_v2.ino` in der Arduino IDE
2. Board: **Arduino Nano**
3. Prozessor: **ATmega328P** (oder "Old Bootloader" je nach Nano-Version)
4. COM-Port auswählen
5. Hochladen

**Test im Serial Monitor** (57600 Baud, Newline):
```
COMMAND:PING          → RESULT:PING:OK:16c5e400-a3b1-11ed-87cd-0800200c9a66
COMMAND:OPEN          → RESULT:OPEN:offen
COMMAND:CLOSE         → RESULT:CLOSE:geschlossen
COMMAND:SETBRIGHTNESS:128 → RESULT:SETBRIGHTNESS:128
```

### 2. Controller-App bauen

**Voraussetzungen:**
- Visual Studio 2022 Community (mit .NET Desktop-Entwicklung)
- .NET 8.0 SDK

**Schritte:**
1. Öffne `03_ControllerApp/FlipFlatPanel.Controller.csproj` in Visual Studio
2. Erstellen → Projektmappe erstellen (F6)
3. Starten mit F5

**Funktionen der App:**
- **Direkt-Modus (Serial)**: Kommuniziert direkt mit dem Arduino zum Testen
- **ASCOM-Modus**: Steuert über den installierten ASCOM-Treiber
- **Helligkeits-Direkteingabe**: Exakte Werte per Textfeld + Enter
- **Protokoll-Fenster**: Zeigt alle gesendeten Befehle und Antworten

### 3. ASCOM-Treiber installieren

**Voraussetzungen:**
- ASCOM Platform 7.x ([Download](https://ascom-standards.org/Downloads/Index.htm))
- Visual Studio 2022 mit ASCOM Platform 7 Templates Extension

**Schritte:**
1. Visual Studio → Neues Projekt → **ASCOM Platform 7 Driver**
2. Device Class: **CoverCalibrator**
3. Vendor/Organization: **FlipFlatPanel**
4. Device Name/Model: **FlipFlatPanel**
5. Erstellen
6. Die generierte `CoverCalibratorHardware.cs` durch die Datei aus `02_AscomDriver/` ersetzen
7. NuGet-Paket **System.IO.Ports** hinzufügen
8. Erstellen (F6)
9. Als Administrator registrieren: `FlipFlatPanel.exe /register`

## Serielles Protokoll

Baudrate: 57600, 8N1, Zeilenende: `\n`

| Befehl | Antwort | Beschreibung |
|--------|---------|-------------|
| `COMMAND:PING` | `RESULT:PING:OK:<GUID>` | Verbindungstest |
| `COMMAND:INFO` | `RESULT:INFO:FlipFlatPanel Firmware v2.0` | Firmware-Info |
| `COMMAND:OPEN` | `RESULT:OPEN:offen` | Cover öffnen |
| `COMMAND:CLOSE` | `RESULT:CLOSE:geschlossen` | Cover schließen |
| `COMMAND:POSITION` | `RESULT:POSITION:<status>` | Aktueller Cover-Status |
| `COMMAND:BRIGHTNESS` | `RESULT:BRIGHTNESS:<0-255>` | Aktuelle Helligkeit |
| `COMMAND:SETBRIGHTNESS:<0-255>` | `RESULT:SETBRIGHTNESS:<0-255>` | Helligkeit setzen |
| `COMMAND:MAXBRIGHTNESS` | `RESULT:MAXBRIGHTNESS:255` | Maximale Helligkeit |

Ungültige Befehle werden mit `ERROR:INVALID_COMMAND` beantwortet.

## Verwendung in N.I.N.A.

1. ASCOM-Treiber registrieren (siehe oben)
2. N.I.N.A. starten
3. Geräte → Flatpanel → ASCOM CoverCalibrator wählen
4. **FlipFlat Panel** auswählen
5. In den Eigenschaften den COM-Port einstellen
6. Verbinden

Für automatisierte Flat-Frames kann N.I.N.A. den Deckel öffnen, die Helligkeit einstellen und nach der Aufnahme wieder schließen.

## Technische Details

### Servo-Steuerung

Die Firmware verwendet manuelle Pulse statt der Arduino Servo-Library, da diese Timer1 belegt und damit den PWM-Ausgang auf Pin 9 stört. Stattdessen wird ein 700ms-Burst von 50Hz-Pulsen gesendet, was für zuverlässige Servo-Bewegung sorgt, ohne die Helligkeitssteuerung zu beeinträchtigen.

### ASCOM CoverCalibrator V2

Der Treiber implementiert das ICoverCalibratorV2-Interface (Platform 7) mit:
- `Connect()` / `Disconnect()` (asynchron, Platform 7)
- `Connected` Property (Rückwärtskompatibilität, Platform 6)
- `OpenCover()` / `CloseCover()` / `HaltCover()`
- `CalibratorOn(brightness)` / `CalibratorOff()`
- `CoverState` / `CoverMoving`
- `CalibratorState` / `CalibratorChanging`
- `Brightness` / `MaxBrightness`

### Servo-Pulsbreiten anpassen

Falls der Servo nicht den gewünschten Winkel erreicht, können die Pulsbreiten in der Firmware angepasst werden:

```cpp
const int SERVO_PULSE_OPEN  = 2500;   // Mikrosekunden (1000-2500)
const int SERVO_PULSE_CLOSE = 540;    // Mikrosekunden (540-1000)
```

## Fehlerbehebung

| Problem | Lösung |
|---------|--------|
| Servo reagiert nicht | Firmware v2.0 flashen; Pulsbreiten prüfen |
| EL-Folie leuchtet nicht | MOSFET-Verdrahtung prüfen; `COMMAND:SETBRIGHTNESS:255` testen |
| ASCOM-Timeout | COM-Port prüfen; Serial Monitor schließen |
| Treiber nicht in N.I.N.A. | `/register` als Administrator ausführen |
| Arduino wird nicht erkannt | CH340/FTDI Treiber installieren |

## Lizenz

MIT License - siehe [LICENSE](LICENSE) Datei.

## Mitwirken

Pull Requests und Issues sind willkommen! Bitte beachte:
- Firmware-Änderungen müssen die Pin-Belegung beibehalten
- Das serielle Protokoll muss abwärtskompatibel bleiben
- ASCOM-Treiber-Änderungen müssen mit ConformU getestet werden
