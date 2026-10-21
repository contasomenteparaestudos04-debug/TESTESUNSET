# Segunda parte: este PC nao tem o winget, entao baixamos os instaladores direto.
# Instala .NET 9 SDK, dotnet-ef e SQL Server Express (instancia SQLEXPRESS).
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$ErrorActionPreference = "Continue"
$tmp = Join-Path $env:TEMP "sunset_setup"
New-Item -ItemType Directory -Force -Path $tmp | Out-Null

# ---- .NET ---- (o SDK 10 ja esta instalado; o projeto usa net9 com RollForward, entao nao precisa do 9)
Write-Host "SDK .NET encontrado:" (dotnet --version)

# ---- SQL Server Express ----
$jaTem = Get-Service -Name 'MSSQL$SQLEXPRESS','MSSQLSERVER' -ErrorAction SilentlyContinue
if ($jaTem) {
    Write-Host "SQL Server ja instalado: $($jaTem.Name). Nada a fazer." -ForegroundColor Green
    return
}
Write-Host "== SQL Server Express (pode demorar uns minutos)" -ForegroundColor Cyan
$sqlSetup = "$tmp\SQL2022-SSEI-Expr.exe"
Invoke-WebRequest "https://go.microsoft.com/fwlink/p/?linkid=2216019&clcid=0x409&culture=en-us&country=us" -OutFile $sqlSetup
$usuario = "$env:USERDOMAIN\$env:USERNAME"
& $sqlSetup /ACTION=Install /QUIET /IACCEPTSQLSERVERLICENSETERMS /INSTANCENAME=SQLEXPRESS /FEATURES=SQLENGINE /SQLSYSADMINACCOUNTS="$usuario" /TCPENABLED=1 | Out-Host

Write-Host ""
Write-Host "Fim. Abra um terminal novo e confira: dotnet --version / dotnet ef --version / flutter --version" -ForegroundColor Green
