$solution_path = '..\src\Solution.slnx'
$unit_tests_project_path = '..\src\Tests\UnitTests'
$integration_tests_project_path = '..\src\Tests\IntegrationTests'
$acceptance_tests_project_path = '..\src\Tests\AcceptanceTests'

if (-not (Test-Path $solution_path)) {
    Write-Error "Solution file not found at $solution_path"
    exit 1
}

if (-not (Test-Path $integration_tests_project_path)) {
    Write-Error "Integration test project not found at $integration_tests_project_path"
    exit 1
}

if (-not (Test-Path $unit_tests_project_path)) {
    Write-Error "Unit test project not found at $unit_tests_project_path"
    exit 1
}

if (-not (Test-Path $acceptance_tests_project_path)) {
    Write-Error "Acceptance test project not found at $acceptance_tests_project_path"
    exit 1
}

Write-Host "******************************************************"
Write-Host "BUILDING SOLUTION"
Write-Host "******************************************************"
dotnet build $solution_path

Write-Host "******************************************************"
Write-Host "RUNNING UNIT TESTS"
Write-Host "******************************************************"
dotnet test $unit_tests_project_path --no-build --no-restore

Write-Host "******************************************************"
Write-Host "RUNNING INTEGRATION TESTS"
Write-Host "******************************************************"
dotnet test $integration_tests_project_path --no-build --no-restore

Write-Host "******************************************************"
Write-Host "RUNNING ACCEPTANCE TESTS"
Write-Host "******************************************************"
dotnet test $acceptance_tests_project_path --no-build --no-restore
