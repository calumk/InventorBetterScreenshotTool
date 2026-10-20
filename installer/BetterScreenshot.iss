; Build after scripts/package.ps1 stages artifacts/plugin.
; CI can override AppVersion with /DAppVersion=0.3.1
#ifndef AppVersion
  #define AppVersion "0.3.1"
#endif
#define AppName "Autodesk Inventor Better Screenshot Tool - By CalumK"
#define AppId "{{9DB7878B-E3CC-456E-9769-75DA28AF669A}"

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=CalumK
AppPublisherURL=https://calumk.com
AppSupportURL=https://calumk.com
AppUpdatesURL=https://calumk.com
DefaultDirName={userappdata}\Autodesk\Inventor 2025\Addins\InventorScreenshotTool
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
OutputDir=..\artifacts\installer
OutputBaseFilename=InventorBetterScreenshotTool-Setup-v{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\InventorScreenshotTool\Assets\BetterScreenshot.ico
UninstallDisplayIcon={app}\BetterScreenshot.ico
VersionInfoVersion={#AppVersion}.0
VersionInfoProductVersion={#AppVersion}
VersionInfoDescription=Better Screenshot Tool installer for Inventor 2025
CloseApplications=no
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\artifacts\plugin\InventorScreenshotTool.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\artifacts\plugin\InventorScreenshotTool.addin"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\artifacts\plugin\BetterScreenshot.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\artifacts\plugin\InventorScreenshotTool.deps.json"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Code]
// We do not force-close Inventor: doing so could destroy unsaved CAD work.
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
  Launched: Boolean;
  PowerShell: String;
  Args: String;
begin
  Result := True;
  PowerShell := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
  Args := '-NoProfile -NonInteractive -Command "if (Get-Process -Name Inventor -ErrorAction SilentlyContinue) { exit 10 } else { exit 0 }"';
  Launched := Exec(PowerShell, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  if Launched and (ResultCode = 10) then
  begin
    MsgBox('Please save your work and close Autodesk Inventor before updating the Better Screenshot Tool.',
      mbError, MB_OK);
    Result := False;
  end;
end;
