; ============================================================
; FlipFlat Panel ASCOM Driver - Inno Setup Installer Script
; ============================================================
; Credits:
;   Ursprüngliches Konzept, Schaltung und Hardware-Design:
;   Moritz Mayer / Dark Matters Discord
;   https://discord.gg/darkmatters
;
; Lizenz: MIT
; ============================================================
; Anleitung:
;   1. ASCOM-Treiber in Visual Studio im RELEASE-Modus erstellen:
;      - Konfiguration auf "Release" umstellen (oben in der Toolbar)
;      - F6 (Erstellen)
;   2. Pfad unten "MyAppSourceDir" ggf. anpassen
;   3. Dieses Script in Inno Setup öffnen (Doppelklick)
;   4. Strg+F9 (Compile)
;   5. Fertiger Installer liegt in "Output\"
; ============================================================

#define MyAppName "FlipFlat Panel ASCOM Driver"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "FlipFlatPanel Project (basiert auf einem Konzept von Moritz Mayer / Dark Matters Discord)"
#define MyAppURL "https://github.com/lindekai/FlipFlatPanel"
#define MyDriverExe "ASCOM.FlipFlatPanel.exe"

; WICHTIG: Diesen Pfad an deine Visual Studio Struktur anpassen!
; Zeigt auf den Release-Build-Ordner des ASCOM-Treibers.
#define MyAppSourceDir "..\02_AscomDriver\FlipFlatPanel.Ascom\FlipFlatPanel.Ascom\bin\Release"

[Setup]
AppId={{A1B2C3D4-FFP1-4567-89AB-CDEF01234567}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppComments=Basiert auf einem Konzept von Moritz Mayer / Dark Matters Discord (https://discord.gg/darkmatters)
DefaultDirName={autopf}\ASCOM\CoverCalibrator\FlipFlatPanel
DefaultGroupName=ASCOM\FlipFlat Panel
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=FlipFlatPanel_AscomDriver_Setup_{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; ASCOM-Treiber MÜSSEN als Administrator installiert werden (wegen COM-Registrierung)
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#MyDriverExe}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Kompletter Inhalt des Release-Ordners (alle DLLs und die Treiber-EXE)
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
; COM-Registrierung nach der Installation
Filename: "{app}\{#MyDriverExe}"; Parameters: "/register"; \
    StatusMsg: "ASCOM-Treiber wird registriert..."; \
    Flags: runhidden waituntilterminated

[UninstallRun]
; COM-Deregistrierung VOR der Deinstallation
Filename: "{app}\{#MyDriverExe}"; Parameters: "/unregister"; \
    Flags: runhidden waituntilterminated; \
    RunOnceId: "UnregisterDriver"

[Code]
// ============================================================
// Prüfe ob ASCOM Platform installiert ist
// ============================================================
function InitializeSetup(): Boolean;
var
  ASCOMInstalled: Boolean;
begin
  Result := True;
  
  // ASCOM Platform Registry-Eintrag prüfen
  ASCOMInstalled := RegKeyExists(HKLM, 'SOFTWARE\ASCOM') or 
                    RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\ASCOM');
  
  if not ASCOMInstalled then
  begin
    if MsgBox('Die ASCOM Platform scheint nicht installiert zu sein.' + #13#10 + #13#10 +
              'Der FlipFlat Panel ASCOM-Treiber benötigt die ASCOM Platform 7 oder höher.' + #13#10 +
              'Du kannst sie kostenlos herunterladen unter:' + #13#10 +
              'https://ascom-standards.org/' + #13#10 + #13#10 +
              'Trotzdem fortfahren?',
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;
