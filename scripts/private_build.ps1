param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

# Verify EF Core tools
if (-not (dotnet tool list --global | Select-String "dotnet-ef")) {
    Write-Error "EF Core tools not found. Run .\install_tools.ps1 first."
    exit 1
}

Write-Host "Adding migration '$MigrationName'..." -ForegroundColor Cyan

dotnet ef migrations add $MigrationName `
    --project "$repoRoot/src/Infrastructure/SQLServer/SQLServer.csproj" `
    --startup-project "$repoRoot/src/Presentation/WebApp/WebApp.csproj" `
    --output-dir Migrations

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration '$MigrationName' added successfully." -ForegroundColor Green
} else {
    Write-Error "Failed to add migration."
    exit 1
}
