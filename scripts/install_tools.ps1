$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

# .NET SDK
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK not found. Install from https://dot.net/download"
    exit 1
}

# Docker - Install if not present
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "Docker not found. Installing Docker..."
    if ($IsLinux) {
        # Install Docker on Linux
        & bash -c "curl -fsSL https://get.docker.com | sh"
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker installation failed. Install manually from https://docker.com"
            exit 1
        }
        Write-Host "Docker installed successfully." -ForegroundColor Green
    } else {
        Write-Error "Docker not found. Install Docker Desktop from https://docker.com/products/docker-desktop"
        exit 1
    }
}

# Start Docker if not running
Write-Host "Checking Docker status..."
$dockerInfo = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker is not running. Attempting to start..."
    $dockerStarted = $false
    
    if ($IsWindows) {
        Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe" -WindowStyle Hidden -ErrorAction SilentlyContinue
    } elseif ($IsLinux) {
        # Try service command first (works in containers), then systemctl
        $result = sudo service docker start 2>&1
        if ($LASTEXITCODE -ne 0) {
            $result = sudo systemctl start docker 2>&1
        }
        # Fix socket permissions if needed
        if (Test-Path "/var/run/docker.sock") {
            sudo chmod 666 /var/run/docker.sock 2>&1 | Out-Null
        }
    } elseif ($IsMacOS) {
        open -a Docker -ErrorAction SilentlyContinue
    }
    
    # Wait for Docker to be ready
    $maxAttempts = 30
    $attempt = 0
    while ($attempt -lt $maxAttempts) {
        Start-Sleep -Seconds 2
        $dockerInfo = docker info 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Docker is running." -ForegroundColor Green
            $dockerStarted = $true
            break
        }
        $attempt++
        Write-Host "Waiting for Docker to start... ($attempt/$maxAttempts)"
    }
    
    if (-not $dockerStarted) {
        Write-Error "Docker could not be started. Integration and acceptance tests require Docker."
        exit 1
    }
} else {
    Write-Host "Docker is already running." -ForegroundColor Green
}

# Node.js
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Error "Node.js not found. Install from https://nodejs.org"
    exit 1
}

# npm
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    Write-Error "npm not found. Install Node.js from https://nodejs.org"
    exit 1
}

# GitHub CLI
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    if ($IsWindows) {
        winget install --id GitHub.cli -e --silent
    } elseif ($IsLinux) {
        Write-Host "Installing GitHub CLI..."
        & bash -c "(type -p wget >/dev/null || (sudo apt update && sudo apt-get install wget -y)) && sudo mkdir -p -m 755 /etc/apt/keyrings && wget -qO- https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo tee /etc/apt/keyrings/githubcli-archive-keyring.gpg > /dev/null && sudo chmod go+r /etc/apt/keyrings/githubcli-archive-keyring.gpg && echo 'deb [arch=`$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main' | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null && sudo apt update && sudo apt install gh -y"
    } elseif ($IsMacOS) {
        brew install gh
    }
    
    # Verify installation
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
        Write-Error "GitHub CLI installation failed. Install manually from https://cli.github.com"
        exit 1
    }
}

# EF Core Tools
Write-Host "Installing/updating EF Core tools..."
dotnet tool update --global dotnet-ef | Out-Null

# Install Angular dependencies
Write-Host "Installing Angular dependencies..."
Push-Location "$repoRoot/src/Presentation/webapp"
npm ci
Pop-Location

# Build AcceptanceTests project
Write-Host "Building AcceptanceTests project..."
dotnet build "$repoRoot/src/Tests/AcceptanceTests/AcceptanceTests.csproj" --configuration Debug

# Install .NET Playwright browsers
Write-Host "Installing .NET Playwright browsers..."
$playwrightScript = "$repoRoot/src/Tests/AcceptanceTests/bin/Debug/net8.0/playwright.ps1"
if (Test-Path $playwrightScript) {
    & pwsh $playwrightScript install chromium
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Playwright browser installation may have failed."
    } else {
        Write-Host "Playwright browsers installed successfully." -ForegroundColor Green
    }
} else {
    Write-Warning "Playwright script not found at $playwrightScript. Browsers may need manual installation."
}

Write-Host "All tools installed successfully." -ForegroundColor Green
