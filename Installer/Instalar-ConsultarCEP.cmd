@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Instalar-ConsultarCEP.ps1"
if errorlevel 1 (
    echo.
    echo Falha ao instalar o Consultar CEP.
    pause
    exit /b 1
)
echo.
echo Consultar CEP instalado ou atualizado com sucesso.
pause
