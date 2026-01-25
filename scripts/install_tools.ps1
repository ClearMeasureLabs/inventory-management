$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

# .NET SDK
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK not found. Install from https://dot.net/download"
    exit 1
}

# Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker not found. Install Docker Desktop from https://docker.com/products/docker-desktop"
    exit 1
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
dotnet tool update --global dotnet-ef | Out-Null

# Install Angular dependencies
Write-Host "Installing Angular dependencies..."
Push-Location "$repoRoot/src/Presentation/webapp"
npm ci
Pop-Location

# Build AcceptanceTests and install Playwright
dotnet build "$repoRoot/src/Tests/AcceptanceTests/AcceptanceTests.csproj" --verbosity quiet
$playwrightScript = Join-Path $repoRoot "src/Tests/AcceptanceTests/bin/Debug/net10.0/playwright.ps1"
& pwsh $playwrightScript install chromium
