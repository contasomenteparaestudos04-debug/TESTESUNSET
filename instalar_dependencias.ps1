# Instala o que o projeto SUNSET precisa no Windows.
# Como usar: botão direito no PowerShell > "Executar como administrador",
# depois: cd "C:\Users\Patrick\Desktop\PIM4Faculdade" ; .\instalar_dependencias.ps1
# Obs.: script escrito sem poder testar no seu PC. Se algum id do winget falhar,
# rode "winget search <nome>" e troque o id.

$ErrorActionPreference = "Continue"

function Instalar($id) {
    Write-Host "== Instalando $id" -ForegroundColor Cyan
    winget install --id $id -e --accept-package-agreements --accept-source-agreements
}

Instalar "Microsoft.DotNet.SDK.9"            # .NET 9 (template do professor)
Instalar "Microsoft.SQLServer.2022.Express"  # SQL Server Express (instância SQLEXPRESS)
Instalar "Microsoft.SQLServerManagementStudio" # SSMS (opcional, para conferir o banco)
Instalar "Git.Git"                           # necessário para o Flutter
Instalar "Google.AndroidStudio"              # SDK Android e emulador para o app

# Flutter: baixa o SDK estável em C:\src\flutter
if (-not (Test-Path "C:\src\flutter")) {
    git clone https://github.com/flutter/flutter.git -b stable C:\src\flutter
}
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\src\flutter\bin", "User")

# Ferramenta do Entity Framework (precisa abrir um terminal novo depois do .NET)
dotnet tool install --global dotnet-ef

Write-Host ""
Write-Host "Pronto. Feche e abra o terminal, depois confira:" -ForegroundColor Green
Write-Host "  dotnet --version"
Write-Host "  dotnet ef --version"
Write-Host "  flutter doctor"
Write-Host "  sqlcmd -S localhost\SQLEXPRESS -E -Q ""select @@version"""
