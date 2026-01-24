$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "******************************************************"
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
