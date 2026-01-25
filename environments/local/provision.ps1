# Read global configuration for project name
$globalConfig = Get-Content -Path "../global.config.json" -Raw | ConvertFrom-Json
$projectName = $globalConfig.Project.Name.ToLower().Replace(" ", "_")

# Read the local configuration
$localConfig = Get-Content -Path "local.config.json" -Raw | ConvertFrom-Json

# Set environment variables from config
$env:SQL_DATABASE = $globalConfig.SqlServer.Database
$env:SQL_SA_PASSWORD = $localConfig.SqlServer.Password
$env:SQL_PORT = $localConfig.SqlServer.Port
$env:RABBITMQ_USER = $localConfig.RabbitMQ.User
$env:RABBITMQ_PASSWORD = $localConfig.RabbitMQ.Password
$env:RABBITMQ_PORT = $localConfig.RabbitMQ.Port
$env:RABBITMQ_MANAGEMENT_PORT = $localConfig.RabbitMQ.ManagementPort
$env:REDIS_PORT = $localConfig.Redis.Port
$env:REDIS_INSIGHT_PORT = $localConfig.RedisInsight.Port

# Run docker compose with the project name
docker compose -p $projectName up -d