$ErrorActionPreference = 'Stop'

$installerDir = $PSScriptRoot
$projectDir = Split-Path $installerDir -Parent
$releaseDir = Join-Path $projectDir 'bin\Release'
$outputPath = Join-Path $releaseDir 'ConsultarCEP-Setup.exe'
$iexpress = Join-Path $env:WINDIR 'System32\iexpress.exe'

if (-not (Test-Path -LiteralPath $releaseDir)) {
    throw "A pasta Release não foi encontrada: $releaseDir"
}

foreach ($arquivo in @('ConsultaCEPs.exe', 'ConsultaCEPs.exe.config')) {
    if (-not (Test-Path -LiteralPath (Join-Path $releaseDir $arquivo))) {
        throw "Arquivo Release não encontrado: $arquivo"
    }
}

$sedPath = Join-Path $env:TEMP ('ConsultarCEP-' + [guid]::NewGuid().ToString() + '.sed')
$releaseExe = Join-Path $releaseDir 'ConsultaCEPs.exe'
$releaseConfig = Join-Path $releaseDir 'ConsultaCEPs.exe.config'
$installScript = Join-Path $installerDir 'Instalar-ConsultarCEP.ps1'
$uninstallScript = Join-Path $installerDir 'Desinstalar-ConsultarCEP.ps1'
$commandScript = Join-Path $installerDir 'Instalar-ConsultarCEP.cmd'

$sed = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=1
HideExtractAnimation=0
UseLongFileName=1
InsideCompressed=1
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=I
InstallPrompt=%InstallPrompt%
DisplayLicense=
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=Instalar o Consultar CEP agora?
FinishMessage=O Consultar CEP foi instalado ou atualizado com sucesso.
FriendlyName=Instalador do Consultar CEP
AppLaunched=Instalar-ConsultarCEP.cmd
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
TargetName=$outputPath
FILE0="Instalar-ConsultarCEP.ps1"
FILE1="Desinstalar-ConsultarCEP.ps1"
FILE2="Instalar-ConsultarCEP.cmd"
FILE3="ConsultaCEPs.exe"
FILE4="ConsultaCEPs.exe.config"
[SourceFiles]
SourceFiles0=$installerDir\
SourceFiles1=$releaseDir\
[SourceFiles0]
%FILE0%=$installScript
%FILE1%=$uninstallScript
%FILE2%=$commandScript
[SourceFiles1]
%FILE3%=$releaseExe
%FILE4%=$releaseConfig
"@

Set-Content -LiteralPath $sedPath -Value $sed -Encoding ASCII
try {
    $process = Start-Process -FilePath $iexpress -ArgumentList @('/N', '/Q', $sedPath) -Wait -PassThru
    if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $outputPath)) {
        throw "O IExpress não conseguiu criar o instalador. Código: $($process.ExitCode)"
    }
}
finally {
    if (Test-Path -LiteralPath $outputPath) {
        Remove-Item -LiteralPath $sedPath -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Warning "Arquivo SED mantido para diagnóstico: $sedPath"
    }
}

Write-Host "Instalador criado em: $outputPath"
