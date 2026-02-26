# Read global configuration for project name
$globalConfig = Get-Content -Path "../global.config.json" -Raw | ConvertFrom-Json
$projectName = $globalConfig.Project.Name.ToLower().Replace(" ", "_")

# Read the local configuration
$localConfig = Get-Content -Path "local.config.json" -Raw | ConvertFrom-Json

# Load secrets from .env file (if not already set via environment variables)
$envFile = Join-Path $PSScriptRoot ".env"
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^\s*([^#][^=]+)=(.*)$') {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            if (-not (Test-Path "env:$key")) {
                Set-Item -Path "env:$key" -Value $value
            }
        }
    }
}

# Set environment variables from config (non-secret values)
$env:SQL_DATABASE = $globalConfig.SqlServer.Database
$env:SQL_PORT = if ($env:SQL_PORT) { $env:SQL_PORT } else { "1433" }
$env:RABBITMQ_PORT = if ($env:RABBITMQ_PORT) { $env:RABBITMQ_PORT } else { "5672" }
$env:RABBITMQ_MANAGEMENT_PORT = if ($localConfig.RabbitMQ.ManagementPort) { $localConfig.RabbitMQ.ManagementPort } else { "15672" }
$env:REDIS_PORT = if ($env:REDIS_PORT) { $env:REDIS_PORT } else { "6379" }
$env:REDIS_INSIGHT_PORT = if ($localConfig.RedisInsight.Port) { $localConfig.RedisInsight.Port } else { "5540" }

# Run docker compose with the project name
docker compose -p "${projectName}_infra" up -d