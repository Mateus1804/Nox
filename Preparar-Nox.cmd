@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\Preparar-Nox.ps1" -GerarBuild
set "NOX_EXIT_CODE=%errorlevel%"
if not "%NOX_EXIT_CODE%"=="0" (
  echo.
  echo A preparacao ou a build nao foi concluida. Consulte a mensagem acima.
)
pause
exit /b %NOX_EXIT_CODE%
