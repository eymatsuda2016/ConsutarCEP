param(
    [Parameter(Mandatory = $true)]
    [string]$CertificatePath,

    [string]$CertificatePassword,

    [string]$TimestampUrl = 'http://timestamp.digicert.com'
)

$ErrorActionPreference = 'Stop'
$releaseDir = Join-Path (Split-Path $PSScriptRoot -Parent) 'bin\Release'
$arquivos = @(
    (Join-Path $releaseDir 'ConsultaCEPs.exe'),
    (Join-Path $releaseDir 'ConsultarCEP-Setup.exe')
)

if (-not (Test-Path -LiteralPath $CertificatePath)) {
    throw "Certificado não encontrado: $CertificatePath"
}

foreach ($arquivo in $arquivos) {
    if (-not (Test-Path -LiteralPath $arquivo)) {
        throw "Artefato Release não encontrado: $arquivo"
    }
}

$signtool = Get-ChildItem -Path @(
    (Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'),
    (Join-Path $env:ProgramFiles 'Windows Kits\10\bin')
) -Filter 'signtool.exe' -Recurse -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $signtool) {
    throw 'signtool.exe não foi encontrado. Instale o Windows SDK para assinar os artefatos.'
}

$argumentos = @('sign', '/fd', 'SHA256', '/f', $CertificatePath, '/tr', $TimestampUrl, '/td', 'SHA256')
if (-not [string]::IsNullOrWhiteSpace($CertificatePassword)) {
    $argumentos += @('/p', $CertificatePassword)
}

foreach ($arquivo in $arquivos) {
    & $signtool.FullName @argumentos $arquivo
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao assinar: $arquivo"
    }

    & $signtool.FullName verify /pa /all $arquivo
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao validar a assinatura: $arquivo"
    }
}

Write-Host 'Artefatos assinados e validados com sucesso.'
