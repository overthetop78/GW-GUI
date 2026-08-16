#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif
#ifndef SourceDir
  #define SourceDir "..\artifacts\publish\win-x64"
#endif
#ifndef OutputDir
  #define OutputDir "..\artifacts"
#endif

[Setup]
AppId={{7B909A70-92B3-48E5-82CB-51A584ECE231}
AppName=GW GUI
AppVersion={#MyAppVersion}
AppPublisher=overthetop78
AppPublisherURL=https://github.com/overthetop78/GW-GUI
DefaultDirName={autopf}\GW GUI
DefaultGroupName=GW GUI
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir={#OutputDir}
OutputBaseFilename=GW-GUI-{#MyAppVersion}-win-x64-setup
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayName=GW GUI
WizardStyle=modern
SetupIconFile=..\src\GWGUI.App\Assets\app-icon.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "chinesesimplified"; MessagesFile: "Languages\ChineseSimplified.isl"
Name: "chinesetraditional"; MessagesFile: "Languages\ChineseTraditional.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "greek"; MessagesFile: "Languages\Greek.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "hungarian"; MessagesFile: "compiler:Languages\Hungarian.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "swedish"; MessagesFile: "compiler:Languages\Swedish.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "romanian"; MessagesFile: "Languages\Romanian.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "arabic"; MessagesFile: "compiler:Languages\Arabic.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "thai"; MessagesFile: "compiler:Languages\Thai.isl"
Name: "indonesian"; MessagesFile: "Languages\Indonesian.isl"
Name: "vietnamese"; MessagesFile: "Languages\Vietnamese.isl"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "Prerequisites\windowsdesktop-runtime-10.0.11-win-x64.exe"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\GW GUI"; Filename: "{app}\gwgui.exe"
Name: "{autodesktop}\GW GUI"; Filename: "{app}\gwgui.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\gwgui.exe"; Description: "{cm:LaunchProgram,GW GUI}"; Flags: nowait postinstall skipifsilent unchecked

[InstallDelete]
; Releases older than 0.1.1 were self-contained and left a private .NET runtime
; beside gwgui.exe. A framework-dependent upgrade must remove those root DLLs:
; otherwise apphost selects the incomplete application-local runtime instead of
; the Microsoft Windows Desktop Runtime installed on the computer.
Type: files; Name: "{app}\*.dll"
Type: files; Name: "{app}\*.deps.json"
Type: files; Name: "{app}\*.runtimeconfig.json"
Type: files; Name: "{app}\GW GUI.exe"
Type: files; Name: "{app}\createdump.exe"

[Code]
const
  DotNetDesktopRuntimeRegistryKey = 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App';
  DotNetDesktopRuntimeMajorPrefix = '10.';
  DotNetDesktopRuntimeInstaller = 'windowsdesktop-runtime-10.0.11-win-x64.exe';

function DotNetDesktopRuntimeInstalled: Boolean;
var
  ValueNames: TArrayOfString;
  Index: Integer;
begin
  Result := False;
  if not RegGetValueNames(HKLM64, DotNetDesktopRuntimeRegistryKey, ValueNames) then Exit;
  for Index := 0 to GetArrayLength(ValueNames) - 1 do
    if Pos(DotNetDesktopRuntimeMajorPrefix, ValueNames[Index]) = 1 then
    begin
      Result := True;
      Exit;
    end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  RuntimeInstallerPath: String;
  ResultCode: Integer;
begin
  Result := '';
  if DotNetDesktopRuntimeInstalled then Exit;

  ExtractTemporaryFile(DotNetDesktopRuntimeInstaller);
  RuntimeInstallerPath := ExpandConstant('{tmp}\') + DotNetDesktopRuntimeInstaller;
  ResultCode := -1;
  if (not Exec(RuntimeInstallerPath, '/install /passive /norestart', '', SW_SHOW,
      ewWaitUntilTerminated, ResultCode)) or (ResultCode <> 0) then
    Result := SysErrorMessage(ResultCode);
end;
