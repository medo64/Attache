#define AppName        GetStringFileInfo('..\Binaries\Attache.exe', 'ProductName')
#define AppVersion     GetStringFileInfo('..\Binaries\Attache.exe', 'ProductVersion')
#define AppFileVersion GetStringFileInfo('..\Binaries\Attache.exe', 'FileVersion')
#define AppCompany     GetStringFileInfo('..\Binaries\Attache.exe', 'CompanyName')
#define AppCopyright   GetStringFileInfo('..\Binaries\Attache.exe', 'LegalCopyright')
#define AppBase        LowerCase(StringChange(AppName, ' ', ''))
#define AppSetupFile   AppBase + StringChange(AppVersion, '.', '')

#define AppVersionEx   StringChange(AppVersion, '0.00', '')
#if "" != VersionHash
#  define AppVersionEx AppVersionEx + " (" + VersionHash + ")"
#endif


[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppCompany}
AppPublisherURL=http://jmedved.com/{#AppBase}/
AppCopyright={#AppCopyright}
VersionInfoProductVersion={#AppVersion}
VersionInfoProductTextVersion={#AppVersionEx}
VersionInfoVersion={#AppFileVersion}
DefaultDirName={pf}\{#AppCompany}\{#AppName}
OutputBaseFilename={#AppSetupFile}
OutputDir=..\Releases
SourceDir=..\Binaries
AppId=JosipMedved_Attache
CloseApplications="yes"
RestartApplications="no"
UninstallDisplayIcon={app}\Attache.exe
AlwaysShowComponentsList=no
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
MergeDuplicateFiles=yes
MinVersion=0,6.01.7200
PrivilegesRequired=admin
ShowLanguageDialog=no
SolidCompression=yes
ChangesAssociations=yes
DisableWelcomePage=yes
LicenseFile=..\Setup\License.rtf


[Messages]
SetupAppTitle=Setup {#AppName} {#AppVersionEx}
SetupWindowTitle=Setup {#AppName} {#AppVersionEx}
BeveledLabel=jmedved.com


[Files]
Source: "Attache.exe";  DestDir: "{app}";  Flags: ignoreversion;
Source: "Attache.pdb";  DestDir: "{app}";  Flags: ignoreversion;
Source: "ReadMe.txt";   DestDir: "{app}";  Attribs: readonly;  Flags: overwritereadonly uninsremovereadonly;
Source: "License.txt";  DestDir: "{app}";  Attribs: readonly;  Flags: overwritereadonly uninsremovereadonly;


[Icons]
Name: "{userstartmenu}\Attache";  Filename: "{app}\Attache.exe"


[Registry]
Root: HKCU;  Subkey: "Software\Josip Medved";          ValueType: none;    Flags: uninsdeletekeyifempty;
Root: HKCU;  Subkey: "Software\Josip Medved\Attache";  ValueType: none;    Flags: deletekey uninsdeletekey;


[Run]
Filename: "{app}\Attache.exe";  Parameters: "/Install";    Flags: runascurrentuser waituntilterminated;
Filename: "{app}\Attache.exe";                             Flags: postinstall nowait skipifsilent runasoriginaluser;  Description: "Launch application now";


[UninstallRun]
Filename: "{app}\Attache.exe";  Parameters: "/Uninstall";  Flags: runascurrentuser waituntilterminated


[Code]

procedure InitializeWizard;
begin
  WizardForm.LicenseAcceptedRadio.Checked := True;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
    ResultCode: Integer;
begin
    Exec(ExpandConstant('{app}\VhdAttachService.exe'), '/Uninstall', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
    Result := Result;
end;
