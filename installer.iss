#define MyAppName "RayCast"
<<<<<<< HEAD
#define MyAppVersion "1.1.0"
=======
#define MyAppVersion "1.0.1"
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
#define MyAppPublisher "RayCast"
#define MyAppExeName "RayCast.exe"

[Setup]
<<<<<<< HEAD
AppId=B8F1E0A0-0B0B-4B0B-8B0B-0B0B0B0B0B0B}
=======
AppId={{B8F1E0A0-0B0B-4B0B-8B0B-0B0B0B0B0B0B}
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no
DisableWelcomePage=no
DisableFinishedPage=no
OutputDir=installer
<<<<<<< HEAD
OutputBaseFilename=RayCast-Setup 1.1.0
=======
OutputBaseFilename=RayCast-Setup 1.0.2
>>>>>>> b759362d32535175e990742b02ab9f1f12ceaced
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
AllowUNCPath=false
AllowNetworkDrive=false
AllowRootDirectory=false
AllowNoIcons=yes
MinVersion=10.0.17763

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net7.0-windows10.0.19041.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net7.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent 