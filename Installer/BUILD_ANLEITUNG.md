# FlipFlat Panel - Build & Installer Anleitung

## Option 1: Einzelne .exe erzeugen (schnell & einfach)

### Schritt 1: Eingabeaufforderung öffnen

Windows-Taste → `cmd` tippen → Enter

### Schritt 2: Zum Projektordner navigieren

```cmd
cd C:\Projekte\FlipFlatPanel\03_ControllerApp
```

(Pfad an deinen Speicherort anpassen)

### Schritt 3: Publizieren

```cmd
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

### Schritt 4: Fertige Datei finden

Die EXE liegt unter:
```
03_ControllerApp\bin\Release\net8.0-windows\win-x64\publish\FlipFlatPanel.Controller.exe
```

Diese einzelne Datei kannst du direkt weitergeben. Sie enthält alles
und braucht keine .NET-Installation auf dem Zielrechner.

---

## Option 2: Professioneller Installer mit Inno Setup

### Voraussetzungen

1. **Inno Setup installieren**: https://jrsoftware.org/isdl.php (Version 6.7.x, kostenlos)
2. Die Controller-App muss zuerst mit Option 1 publiziert worden sein

### Schritt 1: App publizieren (wie Option 1)

```cmd
cd C:\Projekte\FlipFlatPanel\03_ControllerApp
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

### Schritt 2: Installer-Script anpassen

1. Öffne `Installer\ControllerApp_Setup.iss` mit Inno Setup (Doppelklick)
2. Prüfe den Pfad in der Zeile `#define MyAppSourceDir` — er muss auf den
   `publish`-Ordner zeigen
3. Ersetze `YOUR_USERNAME` durch deinen GitHub-Benutzernamen

### Schritt 3: Installer kompilieren

1. In Inno Setup: **Build → Compile** (oder Strg+F9)
2. Der fertige Installer wird erstellt unter:
   ```
   Installer\Output\FlipFlatPanel_Controller_Setup_1.1.0.exe
   ```

### Was der Installer macht

- Installiert die App nach `C:\Program Files\FlipFlatPanel\Controller\`
- Erstellt einen Startmenü-Eintrag
- Erstellt optional eine Desktop-Verknüpfung
- Erstellt einen Uninstaller (zum sauberen Deinstallieren)
- Bietet nach der Installation an, die App direkt zu starten
- Deutsche und englische Installationssprache

---

## Für später: ASCOM-Treiber Installer

Der ASCOM-Treiber braucht einen speziellen Installer, weil er:
- Als Administrator installiert werden muss
- Den COM-Server registrieren muss (`/register`)
- Die ASCOM Platform als Voraussetzung prüfen muss

Dafür erstellen wir ein separates Inno Setup Script, wenn der
ASCOM-Treiber fertig kompiliert ist.
