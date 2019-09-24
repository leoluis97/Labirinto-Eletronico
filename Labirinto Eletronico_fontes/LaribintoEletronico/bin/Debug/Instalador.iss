[Setup]
AppName=Labirinto Eletrônico
AppVersion=1.0
DefaultDirName={userdocs}\Labirinto Eletronico
DefaultGroupName=Labirinto Eletrônico
;UninstallDisplayIcon={app}\MyProg.exe
Compression=lzma2
SolidCompression=yes
;OutputDir=userdocs:Inno Setup Examples Output
OutputBaseFilename=Setup - Labrinto Eletrônico

[Files]
Source: "bass.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Bass.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Drix_joystick.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "records"; DestDir: "{app}"; Flags: ignoreversion
Source: "ja.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "fim.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "beep.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "botao.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "aplausos.wav"; DestDir: "{app}"; Flags: ignoreversion
Source: "vc_redist.x64_2015.exe"; DestDir: "{tmp}"; Flags: ignoreversion
Source: "dotNetFx40_Full_x86_x64_SC.exe"; DestDir: "{tmp}"; Flags: ignoreversion
Source: "LaribintoEletronico.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
;Name: "{group}\My Program"; Filename: "{app}\MyProg.exe"

[Run]
Filename: "{tmp}\dotNetFx40_Full_x86_x64_SC.exe"
Filename: "{tmp}\vc_redist.x64_2015.exe"