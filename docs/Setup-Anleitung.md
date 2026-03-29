# FlipFlat Panel - Projekt-Setup & GitHub Anleitung

## Inhaltsverzeichnis

1. [Überblick Plattformen](#1-überblick-plattformen)
2. [GitHub-Repository erstellen](#2-github-repository-erstellen)
3. [Git installieren](#3-git-installieren)
4. [Windows: Visual Studio 2022 einrichten](#4-windows-visual-studio-2022-einrichten)
5. [Mac: Visual Studio Code einrichten](#5-mac-visual-studio-code-einrichten)
6. [Projekt mit GitHub verbinden](#6-projekt-mit-github-verbinden)
7. [Tägliches Arbeiten mit Git](#7-tägliches-arbeiten-mit-git)
8. [Projektstruktur in Visual Studio](#8-projektstruktur-in-visual-studio)

---

## 1. Überblick Plattformen

| Komponente | Windows | Mac |
|---|---|---|
| Arduino Firmware bearbeiten | Visual Studio / VS Code / Arduino IDE | VS Code / Arduino IDE |
| Controller-App (WPF) bauen | Visual Studio 2022 (nur Windows) | Nur bearbeiten in VS Code* |
| ASCOM-Treiber bauen | Visual Studio 2022 (nur Windows) | Nur bearbeiten in VS Code* |
| Git & GitHub | Voll unterstützt | Voll unterstützt |

> *WPF und ASCOM sind Windows-Technologien. Auf dem Mac kannst du den Code bearbeiten
> und auf GitHub pushen, aber zum Kompilieren und Testen brauchst du einen Windows-Rechner.

---

## 2. GitHub-Repository erstellen

### 2.1 GitHub-Account (falls noch nicht vorhanden)

1. Gehe zu **https://github.com**
2. Klicke **Sign up**
3. Erstelle einen Account (kostenlos)

### 2.2 Neues Repository erstellen

1. Auf GitHub: Klicke oben rechts auf **+** → **New repository**
2. Einstellungen:
   - **Repository name**: `FlipFlatPanel`
   - **Description**: `DIY Flat-Field Panel für Astrofotografie - Arduino Nano, ASCOM CoverCalibrator, N.I.N.A.`
   - **Visibility**: Public (oder Private, wenn du möchtest)
   - **NICHT** ankreuzen: "Add a README file" (wir haben bereits eine)
   - **NICHT** ankreuzen: "Add .gitignore" (wir haben bereits eine)
   - **License**: None (wir haben bereits eine MIT License)
3. Klicke **Create repository**
4. GitHub zeigt dir jetzt Befehle an — **diese Seite offen lassen**, wir brauchen sie gleich!

---

## 3. Git installieren

### Windows

1. Gehe zu **https://git-scm.com/download/win**
2. Lade den Installer herunter und starte ihn
3. Bei der Installation alle Standardeinstellungen belassen, außer:
   - **Default editor**: Wähle "Use Visual Studio Code as Git's default editor" (falls VS Code installiert)
   - **Initial branch name**: Wähle "Override" und tippe `main` ein
4. Nach der Installation: Öffne eine **Eingabeaufforderung** (cmd) und tippe:

```bash
git --version
```

Sollte etwas wie `git version 2.x.x` ausgeben.

5. Git konfigurieren (einmalig):

```bash
git config --global user.name "DEIN NAME"
git config --global user.email "deine.email@beispiel.de"
```

> Verwende die gleiche E-Mail wie bei deinem GitHub-Account!

### Mac

1. Öffne das **Terminal** (Programme → Dienstprogramme → Terminal)
2. Tippe:

```bash
git --version
```

3. Falls Git nicht installiert ist, wirst du aufgefordert die Xcode Command Line Tools zu installieren. Bestätige mit **Installieren**.
4. Git konfigurieren (einmalig):

```bash
git config --global user.name "DEIN NAME"
git config --global user.email "deine.email@beispiel.de"
```

---

## 4. Windows: Visual Studio 2022 einrichten

### 4.1 Installation

Falls noch nicht geschehen:

1. Download: **https://visualstudio.microsoft.com/de/vs/community/** (kostenlos)
2. Im Installer diese Workload auswählen:
   - **.NET Desktop-Entwicklung**
3. Installieren

### 4.2 GitHub-Erweiterung (in Visual Studio integriert)

Visual Studio 2022 hat Git-Unterstützung bereits eingebaut. Für eine bessere GitHub-Integration:

1. Visual Studio öffnen
2. Menü: **Erweiterungen → Erweiterungen verwalten**
3. Suche nach **"GitHub"**
4. Installiere **GitHub Extension for Visual Studio** (falls nicht bereits vorhanden)
5. Visual Studio neu starten

### 4.3 Bei GitHub anmelden

1. In Visual Studio: Menü **Git → Einstellungen**
2. Unter **Konten** oder **Git → GitHub** mit deinem GitHub-Account anmelden
3. Alternativ: Menü **Datei → Kontoeinstellungen → Konto hinzufügen → GitHub**

### 4.4 Projektmappe erstellen

Da wir mehrere Projekte in einer Lösung haben wollen:

1. Visual Studio öffnen → **Neues Projekt erstellen**
2. Suche nach **"Leere Projektmappe"** (Blank Solution)
3. Name: **FlipFlatPanel**
4. Speicherort: Wähle den Ordner **über** dem entpackten FlipFlatPanel-Ordner
   - Beispiel: Wenn dein Projekt unter `C:\Projekte\FlipFlatPanel\` liegt,
     wähle `C:\Projekte\` als Speicherort
5. Erstellen

#### Controller-App hinzufügen

1. Im Projektmappen-Explorer: Rechtsklick auf die Projektmappe → **Hinzufügen → Vorhandenes Projekt**
2. Navigiere zu `03_ControllerApp/FlipFlatPanel.Controller.csproj`
3. Öffnen

#### ASCOM-Treiber hinzufügen

Wenn du den ASCOM-Treiber über das Template erstellt hast:

1. Rechtsklick auf die Projektmappe → **Hinzufügen → Vorhandenes Projekt**
2. Navigiere zum ASCOM-Treiber-Projektordner und wähle die `.csproj`-Datei

#### Arduino-Firmware als Projektmappenordner

1. Rechtsklick auf die Projektmappe → **Hinzufügen → Neuer Projektmappenordner**
2. Name: **01_Firmware**
3. Rechtsklick auf den neuen Ordner → **Hinzufügen → Vorhandenes Element**
4. Navigiere zu `01_Firmware/FlipFlatPanel_v2.ino` und füge es hinzu

---

## 5. Mac: Visual Studio Code einrichten

### 5.1 Installation

1. Download: **https://code.visualstudio.com/**
2. Installieren (DMG öffnen, in Programme ziehen)

### 5.2 Empfohlene Erweiterungen

Öffne VS Code und installiere diese Erweiterungen (Cmd+Shift+X):

| Erweiterung | Zweck |
|---|---|
| **C#** (Microsoft) | C#-Syntax, IntelliSense |
| **C# Dev Kit** (Microsoft) | Projektmappen-Unterstützung |
| **Arduino** (Microsoft) | Arduino-Sketch bearbeiten |
| **GitLens** (GitKraken) | Erweiterte Git-Integration |
| **GitHub Pull Requests** (GitHub) | PRs direkt aus VS Code |

### 5.3 Projekt öffnen

1. VS Code öffnen
2. **Datei → Ordner öffnen** (Cmd+O)
3. Wähle den `FlipFlatPanel`-Ordner
4. VS Code zeigt die gesamte Projektstruktur im Explorer (links)

### 5.4 Einschränkungen auf dem Mac

- Du kannst allen Code **bearbeiten** und auf GitHub **pushen**
- Die WPF-App und der ASCOM-Treiber können auf dem Mac **nicht kompiliert** werden
- Die Arduino-Firmware kann mit der Arduino IDE auf dem Mac geflasht werden
- Zum Testen des ASCOM-Treibers und der WPF-App brauchst du Windows

---

## 6. Projekt mit GitHub verbinden

### 6.1 Erste Verbindung herstellen (Windows oder Mac)

Öffne eine **Eingabeaufforderung** (Windows) oder ein **Terminal** (Mac) und navigiere zum Projektordner:

```bash
# Windows
cd C:\Projekte\FlipFlatPanel

# Mac
cd ~/Projekte/FlipFlatPanel
```

Dann diese Befehle der Reihe nach ausführen:

```bash
# Git-Repository initialisieren
git init

# Alle Dateien hinzufügen
git add .

# Ersten Commit erstellen
git commit -m "Initial commit: Firmware v2.0, ASCOM Driver, Controller App v1.1"

# Hauptbranch auf 'main' setzen
git branch -M main

# GitHub als Remote hinzufügen (URL von deiner GitHub-Seite!)
git remote add origin https://github.com/DEIN_USERNAME/FlipFlatPanel.git

# Auf GitHub hochladen
git push -u origin main
```

> Ersetze `DEIN_USERNAME` mit deinem GitHub-Benutzernamen!

Beim ersten Push wirst du nach deinen GitHub-Zugangsdaten gefragt. Unter Windows öffnet sich eventuell ein Browser-Fenster zur Anmeldung.

### 6.2 Über Visual Studio (Alternative für Windows)

1. In Visual Studio: Menü **Git → Git-Repository erstellen**
2. Wähle **Vorhandenes Remote** und gib die GitHub-Repository-URL ein
3. Klicke **Erstellen und pushen**

Oder falls das Repository lokal bereits existiert:

1. Menü **Git → Repository öffnen**
2. Navigiere zum FlipFlatPanel-Ordner
3. Öffnen
4. Unten in der Statusleiste: Klicke auf **Veröffentlichen** oder **Push**

### 6.3 Über VS Code (Alternative für Mac)

1. In VS Code: Klicke auf das **Source Control** Symbol (links, Zweig-Symbol)
2. Klicke **Repository initialisieren**
3. Gib eine Commit-Nachricht ein: `Initial commit`
4. Klicke den Haken ✓ zum Committen
5. Klicke **Publish Branch** → Wähle **GitHub** → Melde dich an
6. Wähle das bereits erstellte Repository `FlipFlatPanel`

---

## 7. Tägliches Arbeiten mit Git

### Änderungen speichern und hochladen

#### Über die Kommandozeile

```bash
# Status prüfen: Welche Dateien haben sich geändert?
git status

# Alle Änderungen hinzufügen
git add .

# Commit mit Beschreibung
git commit -m "Beschreibung der Änderung"

# Auf GitHub hochladen
git push
```

#### Über Visual Studio (Windows)

1. Unten links in der Statusleiste siehst du einen Stift mit Zahl (geänderte Dateien)
2. Klicke darauf oder gehe zu **Git → Änderungen**
3. Schreibe eine Commit-Nachricht
4. Klicke **Alle committen**
5. Klicke **Push** (Pfeil nach oben) in der Statusleiste

#### Über VS Code (Mac/Windows)

1. Klicke auf das **Source Control** Symbol (links)
2. Du siehst alle geänderten Dateien
3. Klicke **+** bei einzelnen Dateien oder **Stage All Changes**
4. Schreibe eine Commit-Nachricht oben
5. Klicke den **Haken ✓**
6. Klicke **Sync Changes** (oder **Push**)

### Änderungen vom GitHub holen

```bash
git pull
```

Oder in Visual Studio/VS Code: **Pull**-Button klicken.

### Gute Commit-Nachrichten

```
# Gut:
git commit -m "Firmware: Servo-Burst auf 800ms erhöht"
git commit -m "Controller: Helligkeits-Direkteingabe hinzugefügt"
git commit -m "ASCOM: Timeout bei Connect auf 3s erhöht"
git commit -m "README: Schaltplan aktualisiert"

# Schlecht:
git commit -m "Update"
git commit -m "Fixes"
git commit -m "asdf"
```

---

## 8. Projektstruktur in Visual Studio

So sollte der Projektmappen-Explorer am Ende aussehen:

```
Projektmappe 'FlipFlatPanel'
├── 01_Firmware (Projektmappenordner)
│   └── FlipFlatPanel_v2.ino
├── FlipFlatPanel.Ascom (ASCOM Local Server Projekt)
│   ├── CoverCalibratorDriver/
│   │   ├── CoverCalibratorDriver.cs
│   │   ├── CoverCalibratorHardware.cs  ← unsere Datei
│   │   └── SetupDialogForm.cs
│   ├── LocalServer.cs
│   ├── SharedResources.cs
│   └── ...
└── FlipFlatPanel.Controller (WPF-Projekt)
    ├── App.xaml
    └── MainWindow.xaml
```

### Startprojekt festlegen

- Zum Testen der **Controller-App**: Rechtsklick auf `FlipFlatPanel.Controller` → **Als Startprojekt festlegen** → F5
- Zum Testen des **ASCOM-Treibers**: Rechtsklick auf das ASCOM-Projekt → **Als Startprojekt festlegen** → F5 (startet den LocalServer)

---

## Zusammenfassung: Quick-Start

### Erstmaliges Setup (10 Minuten)

1. GitHub-Account erstellen und Repository `FlipFlatPanel` anlegen
2. Git installieren und konfigurieren
3. ZIP entpacken in z.B. `C:\Projekte\FlipFlatPanel\`
4. Terminal öffnen, zum Ordner navigieren
5. `git init` → `git add .` → `git commit -m "Initial commit"` → `git remote add origin ...` → `git push -u origin main`
6. Fertig! Dein Code ist auf GitHub.

### Danach (täglich)

1. Code bearbeiten
2. `git add .` → `git commit -m "Beschreibung"` → `git push`
3. Auf dem anderen Rechner: `git pull`

---

## Tipps

- **Schaltplan hochladen**: Lege dein Schaltplan-Bild in den `docs/`-Ordner und verweise in der README darauf
- **Releases**: Wenn eine Version stabil ist, erstelle auf GitHub ein **Release** (Releases → New Release → Tag vergeben wie `v2.0.0`)
- **Issues**: Nutze GitHub Issues um Bugs und Feature-Wünsche zu tracken
- **Branches**: Für größere Änderungen erstelle einen Branch: `git checkout -b feature/neue-funktion`, arbeite darin, und merge zurück
