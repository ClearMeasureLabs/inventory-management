$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "******************************************************"
Write-Host "BUILDING ANGULAR APP"
Write-Host "******************************************************"
Push-Location "$repoRoot/src/Presentation/webapp"
npm ci
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
npm run build
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location

Write-Host "`n******************************************************"
Write-Host "BUILDING DOCKER IMAGES"
Write-Host "******************************************************"

# Build WebAPI Docker image
Write-Host "Building WebAPI Docker image..."
$env:DOCKER_BUILDKIT = "0"
docker build -f "$repoRoot/src/Presentation/WebAPI/Dockerfile" -t webapi-acceptance-test:latest "$repoRoot"
if ($LASTEXITCODE -ne 0) { exit 1 }

# Build Angular Docker image
Write-Host "Building Angular Docker image..."
docker build -f "$repoRoot/src/Presentation/webapp/Dockerfile" -t angular-acceptance-test:latest "$repoRoot/src/Presentation/webapp"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n******************************************************"
Write-Host "BUILDING SOLUTION"
Write-Host "******************************************************"
dotnet build "$repoRoot/src/Solution.slnx"
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n******************************************************"
Write-Host "RUNNING UNIT TESTS"
Write-Host "******************************************************"
dotnet test "$repoRoot/src/Tests/UnitTests" --no-build --no-restore

Write-Host "`n******************************************************"
Write-Host "RUNNING INTEGRATION TESTS"
Write-Host "******************************************************"
dotnet test "$repoRoot/src/Tests/IntegrationTests" --no-build --no-restore

Write-Host "`n******************************************************"
Write-Host "RUNNING ACCEPTANCE TESTS"
Write-Host "******************************************************"
dotnet test "$repoRoot/src/Tests/AcceptanceTests" --no-build --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`nAll tests completed successfully." -ForegroundColor Green
