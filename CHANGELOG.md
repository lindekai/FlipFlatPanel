# Changelog

Alle relevanten Änderungen am Projekt.

## [Unreleased]

### Firmware v3.1
- 🔌 **Servo-Pin D7 → D8** – passend zur Platine. An D7 liegt dort die Heizfolie, mit v3.0 hätte sich der Servo auf der Platine nicht bewegt.
- 💾 **Letzte Cover-Position im EEPROM** – nach Reset oder Verbinden meldet `POSITION` sofort offen/geschlossen, die erste Fahrt läuft mit Rampe statt zu springen. Neue EEPROM-Adresse 7, gespeicherte Servo-Werte bleiben beim Update erhalten.

### ASCOM-Treiber
- ⏱️ `OpenCover`/`CloseCover` arbeiten asynchron (ICoverCalibratorV2), Fortschritt über `CoverState`/`CoverMoving`.
- `HaltCover` wirft `MethodNotImplementedException`, wie die Spezifikation es verlangt.
- Setup-Dialog: Namespace korrigiert, wirkungslose Servo-Felder entfernt – Servo-Einstellungen kommen aus der Controller-App.

### Controller-App
- OPEN/CLOSE warten bis zu 30 s (vorher 5 s – bei Standard-Geschwindigkeit dauert eine Fahrt etwa 9 s).
- Immer nur ein serieller Befehl gleichzeitig; „Speichern“ wartet auf „Übernehmen“.
- ASCOM-Modus: Servo-Regler gesperrt, Fahrtende über `CoverMoving`.

### Projekt
- 🎨 README mit Hero-Banner, Badges und vollständiger Protokoll-Tabelle.
- CI (Firmware kompilieren, Controller-App bauen), Issue- und PR-Vorlagen, CONTRIBUTING.
- 📚 Doku: EEPROM-Belegung, bekannte Einschränkungen (Heizfolie D7, kein HaltCover), CI-Umfang.

## [V.1.1.0](https://github.com/lindekai/FlipFlatPanel/releases/tag/V.1.1.0) – 2026-05-06 (Pre-Release)
- ASCOM-Treiber 1.1.0 (Installer).

## [V.1.0.0](https://github.com/lindekai/FlipFlatPanel/releases/tag/V.1.0.0) – 2026-05-01
- Erste Veröffentlichung: Firmware v3.0, ASCOM-Treiber 1.0.1 und Controller-App 1.2.1 als Installer.
