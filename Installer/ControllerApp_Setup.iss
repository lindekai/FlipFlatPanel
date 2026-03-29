; ============================================================
; FlipFlat Panel Controller - Inno Setup Installer Script
; ============================================================
; Anleitung:
;   1. Controller-App in Visual Studio als Release publizieren:
;      dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
;   2. Dieses Script in Inno Setup öffnen (Doppelklick)
;   3. Pfade unten anpassen falls nötig
;   4. Auf "Compile" klicken (Strg+F9)
;   5. Der fertige Installer liegt dann im "Output"-Ordner
; ============================================================

#define MyAppName "FlipFlat Panel Controller"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "FlipFlatPanel Project"
#define MyAppURL "https://github.com/YOUR_USERNAME/FlipFlatPanel"
#define MyAppExeName "FlipFlatPanel.Controller.exe"

; WICHTIG: Diesen Pfad anpassen!
; Zeigt auf den Publish-Ordner nach dem dotnet publish Befehl
#define MyAppSourceDir "03_ControllerApp\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{16C5E400-A3B1-11ED-87CD-0800200C9A66}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
DefaultDirName={autopf}\FlipFlatPanel\Controller
DefaultGroupName=FlipFlat Panel
AllowNoIcons=yes
; Installer-Ausgabe
OutputDir=Installer\Output
OutputBaseFilename=FlipFlatPanel_Controller_Setup_{#MyAppVersion}
; Kompression
Compression=lzma2/ultra64
SolidCompression=yes
; Aussehen
WizardStyle=modern
; 64-Bit
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Benötigt keine Admin-Rechte
PrivilegesRequired=lowest
; Lizenz (optional - auskommentieren wenn nicht gewünscht)
; LicenseFile=LICENSE
; Uninstaller
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; GroupDescription: "Zusätzliche Verknüpfungen:"
Name: "startmenu"; Description: "Startmenü-Eintrag erstellen"; GroupDescription: "Zusätzliche Verknüpfungen:"; Flags: checked

[Files]
; Die publizierte Single-File EXE
Source: "{#MyAppSourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; Falls weitere Dateien neben der EXE liegen (abhängig von publish-Einstellungen)
Source: "{#MyAppSourceDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#MyAppSourceDir}\*.json"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
; Startmenü
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startmenu
Name: "{group}\{#MyAppName} deinstallieren"; Filename: "{uninstallexe}"; Tasks: startmenu
; Desktop
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Nach der Installation optional starten
Filename: "{app}\{#MyAppExeName}"; Description: "{#MyAppName} jetzt starten"; Flags: nowait postinstall skipifsilent

[Code]
// Prüfe ob die App bereits läuft
function InitializeSetup(): Boolean;
begin
  Result := True;
  // Optional: Prüfen ob eine alte Version läuft
end;
