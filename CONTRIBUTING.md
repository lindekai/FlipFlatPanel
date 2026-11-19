# Mitmachen

Danke für dein Interesse! Besonders hilfreich sind:

- **Erfahrungen vom Nachbau** – andere Servos, EL-Folien, Gehäuse, Teleskop-Durchmesser
- **Tests der Platine** – sie ist noch nicht final getestet, jede Rückmeldung hilft
- **Fehlerberichte** über die Issue-Vorlage, mit Versionen, Hardware und Protokoll
- **Grafiken** – der README-Banner wird aus `tools/hero.py` erzeugt, bitte dort ändern statt im SVG

## Regeln

- **Pin-Belegung beibehalten:** Servo an D8, EL-Folie an D9. Pin 9 braucht Timer1 für die PWM, deshalb erzeugt die Firmware die Servo-Pulse selbst – bitte keine Servo-Library einbauen.
- **Serielles Protokoll abwärtskompatibel halten:** Bestehende Befehle und Antworten nicht ändern, neue Befehle in der Protokoll-Tabelle der [README](README.md#serielles-protokoll) ergänzen.
- **ASCOM-Treiber mit [ConformU](https://github.com/ASCOMInitiative/ConformU) testen**, bevor du einen Pull Request stellst.
- Code im Stil der umgebenden Dateien, Kommentare auf Deutsch. Bestehende Zeilenenden (CRLF) beibehalten.
- Ein Pull Request pro Thema, kurze Beschreibung, was und warum. Die [CI](.github/workflows/ci.yml) muss grün sein.

## Entwicklung

| Teil | Werkzeug | Bauen |
|------|----------|-------|
| Firmware | Arduino IDE 2.x oder `arduino-cli` | Board „Arduino Nano“, `01_Firmware/FlipFlatPanel_v3.ino` |
| Controller-App | .NET 8 SDK (Windows) | `dotnet build 03_ControllerApp/FlipFlatPanel.Controller.csproj` |
| ASCOM-Treiber | Visual Studio 2022 + ASCOM Platform 7 Templates | siehe [README](README.md#4-ascom-treiber-bauen) |
| Installer | Inno Setup 6 | Skripte in `Installer/Build_Scripte/` |
