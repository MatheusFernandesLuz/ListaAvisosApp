; Script Inno Setup para o instalador do "Lista de Avisos Diversos".
;
; Como usar:
;  1. Rode primeiro "dotnet publish" (veja README.md) para gerar os
;     arquivos em bin\Release\net8.0-windows\win-x64\publish
;  2. Abra ESTE arquivo no Inno Setup Compiler (clique duas vezes nele
;     depois de instalar o Inno Setup) e clique em Build > Compile (Ctrl+F9)
;  3. O instalador final aparece na pasta "Output" ao lado deste script

#define MyAppName "Lista de Avisos Diversos"
#define MyAppVersion "1.0"
#define MyAppPublisher "CCB Ji-paraná"
#define MyAppExeName "ListaAvisosApp.exe"

[Setup]
AppId={{B6E2B6F0-6E1C-4A9C-9B0A-000000000001}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
; Instala na pasta do usuário (não exige ser Administrador na máquina)
DefaultDirName={localappdata}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=Output
OutputBaseFilename=ListaAvisosApp_Instalador
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=icon.ico

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Atalhos adicionais:"

[Files]
Source: "bin\Release\net10.0-windows7.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName} agora"; Flags: nowait postinstall skipifsilent
