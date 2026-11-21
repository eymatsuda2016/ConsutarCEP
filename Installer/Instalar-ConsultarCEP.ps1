$ErrorActionPreference = 'Stop'

$appName = 'ConsultarCEP'
$displayVersion = '1.0.0'
$installDir = Join-Path $env:LOCALAPPDATA 'Programs\ConsultarCEP'
$sourceDir = Join-Path $PSScriptRoot '..\bin\Release'
$exeName = 'ConsultaCEPs.exe'
$sourceExe = Join-Path $sourceDir $exeName
if (-not (Test-Path -LiteralPath $sourceExe)) {
    $sourceDir = $PSScriptRoot
    $sourceExe = Join-Path $sourceDir $exeName
}
$sourceDir = (Resolve-Path -LiteralPath $sourceDir).Path
$targetExe = Join-Path $installDir $exeName
$uninstallerSource = Join-Path $PSScriptRoot 'Desinstalar-ConsultarCEP.ps1'
$uninstallerTarget = Join-Path $installDir 'Desinstalar-ConsultarCEP.ps1'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\ConsultarCEP'

if (-not (Test-Path -LiteralPath $sourceExe)) {
    throw "Versão Release não encontrada em: $sourceExe"
}

Get-Process -Name 'ConsultaCEPs' -ErrorAction SilentlyContinue | Stop-Process -Force
New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Copy-Item -Path (Join-Path $sourceDir '*') -Destination $installDir -Recurse -Force
Copy-Item -LiteralPath $uninstallerSource -Destination $uninstallerTarget -Force

$shell = New-Object -ComObject WScript.Shell
$desktopDir = $shell.SpecialFolders.Item('Desktop')
$startMenuDir = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
$desktopShortcut = Join-Path $desktopDir 'Consultar CEP.lnk'
$startMenuShortcut = Join-Path $startMenuDir 'Consultar CEP.lnk'

New-Item -ItemType Directory -Path $startMenuDir -Force | Out-Null
foreach ($shortcutPath in @($desktopShortcut, $startMenuShortcut)) {
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $targetExe
    $shortcut.WorkingDirectory = $installDir
    $shortcut.Description = 'Consultar CEP por endereço'
    $shortcut.IconLocation = "$targetExe,0"
    $shortcut.Save()
}

New-Item -Path $uninstallKey -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name 'DisplayName' -Value 'Consultar CEP' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name 'DisplayVersion' -Value $displayVersion -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name 'Publisher' -Value 'Eduardo Y. Matsuda' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name 'InstallLocation' -Value $installDir -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name 'UninstallString' -Value "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$uninstallerTarget`"" -PropertyType String -Force | Out-Null

Start-Process -FilePath $targetExe
Write-Host "Consultar CEP instalado/atualizado em: $installDir"
