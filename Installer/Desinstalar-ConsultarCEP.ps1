$ErrorActionPreference = 'Stop'

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\ConsultarCEP'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\ConsultarCEP'
$shell = New-Object -ComObject WScript.Shell
$desktopDir = $shell.SpecialFolders.Item('Desktop')
$desktopShortcut = Join-Path $desktopDir 'Consultar CEP.lnk'
$startMenuShortcut = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\Consultar CEP.lnk'

Get-Process -Name 'ConsultaCEPs' -ErrorAction SilentlyContinue | Stop-Process -Force
Remove-Item -LiteralPath $desktopShortcut -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $startMenuShortcut -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $uninstallKey -Recurse -Force -ErrorAction SilentlyContinue

$cleanupScript = Join-Path $env:TEMP ('ConsultarCEP-remover-' + [guid]::NewGuid().ToString() + '.ps1')
$cleanup = @"
Start-Sleep -Seconds 2
Remove-Item -LiteralPath '$installDir' -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath '$cleanupScript' -Force -ErrorAction SilentlyContinue
"@
Set-Content -LiteralPath $cleanupScript -Value $cleanup -Encoding UTF8
Start-Process powershell.exe -ArgumentList '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $cleanupScript -WindowStyle Hidden
Write-Host 'Consultar CEP desinstalado.'
