param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$dbContextProject = Join-Path $repoRoot "src/Infrastructure/SQLServer/SQLServer.csproj"
$startupProject = Join-Path $repoRoot "src/Presentation/WebApp/WebApp.csproj"

# Ensure EF Core tools are installed
Write-Host "Checking for EF Core tools..." -ForegroundColor Cyan
$efToolInstalled = dotnet tool list --global | Select-String "dotnet-ef"
if (-not $efToolInstalled) {
    Write-Host "Installing EF Core tools globally..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install EF Core tools." -ForegroundColor Red
        exit 1
    }
    Write-Host "EF Core tools installed successfully." -ForegroundColor Green
} else {
    Write-Host "EF Core tools already installed." -ForegroundColor Green
}

Write-Host "Adding migration '$MigrationName'..." -ForegroundColor Cyan

dotnet ef migrations add $MigrationName `
    --project $dbContextProject `
    --startup-project $startupProject `
    --output-dir Migrations

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration '$MigrationName' added successfully." -ForegroundColor Green
} else {
    Write-Host "Failed to add migration." -ForegroundColor Red
    exit 1
}
