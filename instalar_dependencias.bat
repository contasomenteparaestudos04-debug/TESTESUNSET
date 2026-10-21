@echo off
REM Abre o instalador em PowerShell (pede permissao de administrador)
powershell -NoProfile -Command "Start-Process powershell -Verb RunAs -ArgumentList '-NoProfile -ExecutionPolicy Bypass -NoExit -File \"%~dp0instalar_dependencias.ps1\"'"
