; ============================================================
; FlipFlat Panel Controller App - Inno Setup Installer Script
; ============================================================
; Credits:
;   Ursprüngliches Konzept, Schaltung und Hardware-Design:
;   Moritz Mayer / Dark Matters Discord
;   https://discord.gg/darkmatters
;
; Lizenz: MIT
; ============================================================
; Anleitung:
;   1. Controller-App publizieren:
;      cd 03_ControllerApp
;      dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
;   2. Dieses Script in Inno Setup öffnen
;   3. Strg+F9 (Compile)
;   4. Fertiger Installer liegt in "Output\"
; ============================================================

#define MyAppName "FlipFlat Panel Controller"
#define MyAppVersion "1.2.0"
#define MyAppPublisher "FlipFlatPanel Project (basiert auf einem Konzept von Moritz Mayer / Dark Matters Discord)"
#define MyAppURL "https://github.com/lindekai/FlipFlatPanel"
#define MyAppExeName "FlipFlatPanel.Controller.exe"

; WICHTIG: Pfad zum publish-Ordner
#define MyAppSourceDir "..\03_ControllerApp\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{16C5E400-A3B1-11ED-87CD-0800200C9A66}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppComments=Basiert auf einem Konzept von Moritz Mayer / Dark Matters Discord (https://discord.gg/darkmatters)
DefaultDirName={autopf}\FlipFlatPanel\Controller
DefaultGroupName=FlipFlat Panel
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=FlipFlatPanel_Controller_Setup_{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; GroupDescription: "Zusätzliche Verknüpfungen:"
Name: "startmenu"; Description: "Startmenü-Eintrag erstellen"; GroupDescription: "Zusätzliche Verknüpfungen:"

[Files]
Source: "{#MyAppSourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSourceDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#MyAppSourceDir}\*.json"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startmenu
Name: "{group}\{#MyAppName} deinstallieren"; Filename: "{uninstallexe}"; Tasks: startmenu
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{#MyAppName} jetzt starten"; Flags: nowait postinstall skipifsilent
