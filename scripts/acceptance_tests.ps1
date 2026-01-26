# Acceptance Tests Script
# Deploys the application stack and runs Playwright acceptance tests

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$localEnvPath = Join-Path $repoRoot "environments/local"

Write-Host ""
Write-Host "******************************************************" -ForegroundColor Cyan
Write-Host "DEPLOYING APPLICATION STACK" -ForegroundColor Cyan
Write-Host "******************************************************" -ForegroundColor Cyan

# Run the local deploy script
$deployScript = Join-Path $localEnvPath "deploy.ps1"
if (-not (Test-Path $deployScript)) {
    Write-Error "Deploy script not found at: $deployScript"
    exit 1
}

& pwsh -ExecutionPolicy Bypass -File $deployScript
if ($LASTEXITCODE -ne 0) {
    Write-Error "Deployment failed with exit code $LASTEXITCODE"
    exit 1
}

Write-Host ""
Write-Host "******************************************************" -ForegroundColor Cyan
Write-Host "RUNNING ACCEPTANCE TESTS" -ForegroundColor Cyan
Write-Host "******************************************************" -ForegroundColor Cyan

# Run the Playwright acceptance tests
$acceptanceTestsProject = Join-Path $repoRoot "src/Tests/AcceptanceTests/AcceptanceTests.csproj"
dotnet test $acceptanceTestsProject --logger "console;verbosity=normal"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Acceptance tests FAILED." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "******************************************************" -ForegroundColor Green
Write-Host "ACCEPTANCE TESTS PASSED" -ForegroundColor Green
Write-Host "******************************************************" -ForegroundColor Green
