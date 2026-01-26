# Local Docker Deployment Script
# Builds and deploys the WebApp and WebAPI containers

# Get the directory where this script is located and change to it
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$originalLocation = Get-Location
Set-Location $scriptDir

function Cleanup {
    Set-Location $originalLocation
}

# Read global configuration for project name
$globalConfig = Get-Content -Path "../global.config.json" -Raw | ConvertFrom-Json
$projectName = $globalConfig.Project.Name.ToLower().Replace(" ", "_")

# Read the local configuration
$localConfig = Get-Content -Path "local.config.json" -Raw | ConvertFrom-Json

# Define infrastructure container names
$infraContainers = @("sqlserver", "rabbitmq", "redis", "redis-insight")

# Step 1: Check Docker status
Write-Host ""
Write-Host "Step 1: Checking Docker status..." -ForegroundColor Cyan
$dockerInfo = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker is not running. Please start Docker and try again." -ForegroundColor Red
    Cleanup
    exit 1
}
Write-Host "Docker is running." -ForegroundColor Green

# Step 2: Check infrastructure containers
Write-Host ""
Write-Host "Step 2: Checking infrastructure containers..." -ForegroundColor Cyan
$missingContainers = @()
foreach ($container in $infraContainers) {
    $status = docker inspect -f '{{.State.Running}}' $container 2>&1
    if ($status -ne "true") {
        $missingContainers += $container
    }
}

if ($missingContainers.Count -gt 0) {
    Write-Host "Missing or stopped containers: $($missingContainers -join ', ')" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Step 2a: Running provision.ps1 to start infrastructure..." -ForegroundColor Cyan
    & "./provision.ps1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to provision infrastructure." -ForegroundColor Red
        Cleanup
        exit 1
    }
    Write-Host "Infrastructure provisioned successfully." -ForegroundColor Green
} else {
    Write-Host "All infrastructure containers are running." -ForegroundColor Green
}

# Set environment variables for docker compose
$env:SQL_DATABASE = $globalConfig.SqlServer.Database
$env:SQL_SA_PASSWORD = $localConfig.SqlServer.Password
$env:SQL_USER = $localConfig.SqlServer.User
$env:SQL_PORT = $localConfig.SqlServer.Port
$env:RABBITMQ_USER = $localConfig.RabbitMQ.User
$env:RABBITMQ_PASSWORD = $localConfig.RabbitMQ.Password
$env:RABBITMQ_PORT = $localConfig.RabbitMQ.Port
$env:REDIS_PORT = $localConfig.Redis.Port
$env:WEBAPI_PORT = "5000"
$env:WEBAPP_PORT = "4200"

# Disable BuildKit to avoid transient build issues
$env:DOCKER_BUILDKIT = "0"

# Step 3: Stop and remove existing application containers
Write-Host ""
Write-Host "Step 3: Stopping existing application containers..." -ForegroundColor Cyan
docker compose -f docker-compose.app.yml -p "${projectName}_app" down --remove-orphans 2>$null
# Also remove any orphaned containers with hardcoded names
docker stop webapp webapi 2>$null
docker rm webapp webapi 2>$null
Write-Host "Existing containers removed." -ForegroundColor Green

# Step 4: Build WebAPI Docker image
Write-Host ""
Write-Host "Step 4: Building WebAPI Docker image..." -ForegroundColor Cyan
docker compose -f docker-compose.app.yml -p "${projectName}_app" build webapi
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to build WebAPI image." -ForegroundColor Red
    Cleanup
    exit 1
}
Write-Host "WebAPI image built successfully." -ForegroundColor Green

# Step 5: Build WebApp Docker image
Write-Host ""
Write-Host "Step 5: Building WebApp Docker image..." -ForegroundColor Cyan
docker compose -f docker-compose.app.yml -p "${projectName}_app" build webapp
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to build WebApp image." -ForegroundColor Red
    Cleanup
    exit 1
}
Write-Host "WebApp image built successfully." -ForegroundColor Green

# Step 6: Start application containers
Write-Host ""
Write-Host "Step 6: Starting application containers..." -ForegroundColor Cyan
docker compose -f docker-compose.app.yml -p "${projectName}_app" up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to start application containers." -ForegroundColor Red
    Cleanup
    exit 1
}
Write-Host "Application containers started successfully." -ForegroundColor Green

# Deployment complete
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Application URLs:" -ForegroundColor Cyan
Write-Host "  WebApp:  http://localhost:$($env:WEBAPP_PORT)" -ForegroundColor White
Write-Host "  WebAPI:  http://localhost:$($env:WEBAPI_PORT)" -ForegroundColor White
Write-Host "  Swagger: http://localhost:$($env:WEBAPI_PORT)/swagger" -ForegroundColor White
Write-Host ""

Cleanup
