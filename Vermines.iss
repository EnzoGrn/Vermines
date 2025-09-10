; -----------------------------
; Script Inno Setup for Vermines
; -----------------------------

[Setup]
; Name display in the installer
AppName=VerminesInstaller
; Version display
AppVersion=2.3.0
; Company (facultatif)
AppPublisher=OMGG
; Website (facultatif)
AppPublisherURL=http://91.134.33.129/

; Default folder of the installation
DefaultDirName={pf}\Vermines

; Name of the output file (in OutputDir)
OutputDir=.\Installer
OutputBaseFilename=Vermines

; Installer icon (optionnal)
; SetupIconFile=Build\Vermines.ico

Compression=zip
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Copie uniquement les fichiers du build Unity
Source: "build\StandaloneWindows64\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion

[Icons]
; Shortcut in the starting menu
Name: "{group}\Vermines"; Filename: "{app}\MonJeu.exe"

; Desktop Shortcut (if enabled in options)
Name: "{commondesktop}\Vermines"; Filename: "{app}\Vermines.exe"

[Run]
; Launche the game when installation end (optionnal)
Filename: "{app}\Vermines.exe"; Description: "{cm:LaunchProgram,Vermines}"; Flags: nowait postinstall skipifsilent
