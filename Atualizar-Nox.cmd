@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\Atualizar-Nox.ps1"
set "NOX_EXIT_CODE=%errorlevel%"
if not "%NOX_EXIT_CODE%"=="0" echo A atualizacao nao foi concluida. Consulte a mensagem acima.
pause
exit /b %NOX_EXIT_CODE%
