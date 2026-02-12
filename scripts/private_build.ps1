$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

try {
    Write-Host "******************************************************"
    Write-Host "INSTALLING TOOLS AND DEPENDENCIES"
    Write-Host "******************************************************"
    & "$PSScriptRoot/install_tools.ps1"
    if ($LASTEXITCODE -ne 0) { 
        Write-Warning "Tool installation had issues. Continuing with build..."
    }

    Write-Host "`n******************************************************"
    Write-Host "BUILDING ANGULAR APP"
    Write-Host "******************************************************"
    Push-Location "$repoRoot/src/Presentation/webapp"
    npm ci
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
    npm run build
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
    Pop-Location

    Write-Host "`n******************************************************"
    Write-Host "BUILDING SOLUTION"
    Write-Host "******************************************************"
    dotnet build "$repoRoot/src/Solution.slnx"
    if ($LASTEXITCODE -ne 0) { exit 1 }

    Write-Host "`n******************************************************"
    Write-Host "RUNNING UNIT TESTS"
    Write-Host "******************************************************"
    dotnet test "$repoRoot/src/Tests/UnitTests" --no-build --no-restore
    if ($LASTEXITCODE -ne 0) { exit 1 }

    Write-Host "`n******************************************************"
    Write-Host "RUNNING INTEGRATION TESTS"
    Write-Host "******************************************************"
    dotnet test "$repoRoot/src/Tests/IntegrationTests" --no-build --no-restore
    if ($LASTEXITCODE -ne 0) { exit 1 }

    Write-Host "`n******************************************************"
    Write-Host "RUNNING ANGULAR TESTS"
    Write-Host "******************************************************"
    Push-Location "$repoRoot/src/Presentation/webapp"
    npm test -- --watch=false --browsers=ChromeHeadless
    $angularTestResult = $LASTEXITCODE
    Pop-Location
    
    if ($angularTestResult -ne 0) {
        Write-Host "Angular tests failed" -ForegroundColor Red
        exit 1
    }

    Write-Host "`nAll tests completed successfully." -ForegroundColor Green

} catch {
    Write-Host "Build failed with error: $_" -ForegroundColor Red
    exit 1
}
