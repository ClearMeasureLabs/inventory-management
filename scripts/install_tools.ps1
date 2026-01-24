$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "Installing development tools..." -ForegroundColor Cyan

# EF Core Tools
Write-Host "`n[1/3] EF Core tools..." -ForegroundColor Yellow
dotnet tool update --global dotnet-ef | Out-Null
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to install EF Core tools."; exit 1 }
Write-Host "  EF Core tools ready." -ForegroundColor Green

# Build AcceptanceTests (required for Playwright script)
Write-Host "`n[2/3] Building AcceptanceTests..." -ForegroundColor Yellow
dotnet build "$repoRoot/src/Tests/AcceptanceTests/AcceptanceTests.csproj" --verbosity quiet
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to build AcceptanceTests."; exit 1 }
Write-Host "  AcceptanceTests built." -ForegroundColor Green

# Playwright browsers
Write-Host "`n[3/3] Playwright browsers..." -ForegroundColor Yellow
& "$repoRoot/src/Tests/AcceptanceTests/bin/Debug/net10.0/playwright.ps1" install chromium
if ($LASTEXITCODE -ne 0) { Write-Warning "Playwright installation may have issues." }
else { Write-Host "  Playwright browsers ready." -ForegroundColor Green }

# Verify Docker (required for Testcontainers)
Write-Host "`nVerifying prerequisites..." -ForegroundColor Yellow
$docker = Get-Command docker -ErrorAction SilentlyContinue
if (-not $docker) {
    Write-Warning "Docker not found. Install Docker Desktop to run acceptance tests."
} else {
    Write-Host "  Docker found." -ForegroundColor Green
}

Write-Host "`nDone! You can now run:" -ForegroundColor Cyan
Write-Host "  .\build_and_test.ps1" -ForegroundColor Gray
Write-Host "  .\add_migration.ps1 -MigrationName <name>" -ForegroundColor Gray
