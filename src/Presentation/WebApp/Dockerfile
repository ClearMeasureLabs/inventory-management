# Build stage
# Build context should be the repository root (parent of src/)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore
COPY src/Solution.slnx ./
COPY src/Core/Domain/Domain.csproj Core/Domain/
COPY src/Core/Application/Application.csproj Core/Application/
COPY src/Infrastructure/Bootstrap/Bootstrap.csproj Infrastructure/Bootstrap/
COPY src/Infrastructure/SQLServer/SQLServer.csproj Infrastructure/SQLServer/
COPY src/Infrastructure/Redis/Redis.csproj Infrastructure/Redis/
COPY src/Infrastructure/RabbitMQ/RabbitMQ.csproj Infrastructure/RabbitMQ/
COPY src/Presentation/WebApp/WebApp.csproj Presentation/WebApp/
COPY src/Presentation/WebApp.Client/WebApp.Client.csproj Presentation/WebApp.Client/

# Restore dependencies
RUN dotnet restore Presentation/WebApp/WebApp.csproj

# Copy source code
COPY src/Core/ Core/
COPY src/Infrastructure/ Infrastructure/
COPY src/Presentation/ Presentation/

# Copy config files to the location expected by the csproj relative paths
# Bootstrap.csproj references ../../../environments/ from /src/Infrastructure/Bootstrap/
# So we need /environments/ at the root level
COPY environments/ /environments/

# Build and publish
RUN dotnet publish Presentation/WebApp/WebApp.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks and wait script
RUN apt-get update && apt-get install -y --no-install-recommends curl netcat-openbsd && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=build /app/publish .

# Create startup script that waits for dependencies
RUN echo '#!/bin/bash\n\
set -e\n\
\n\
echo "Waiting for SQL Server..."\n\
until nc -z ${SqlServer__Host:-sqlserver} ${SqlServer__Port:-1433} 2>/dev/null; do\n\
  sleep 1\n\
done\n\
echo "SQL Server is ready"\n\
\n\
echo "Waiting for Redis..."\n\
until nc -z ${Redis__Host:-redis} ${Redis__Port:-6379} 2>/dev/null; do\n\
  sleep 1\n\
done\n\
echo "Redis is ready"\n\
\n\
echo "Waiting for RabbitMQ..."\n\
until nc -z ${RabbitMQ__Host:-rabbitmq} ${RabbitMQ__Port:-5672} 2>/dev/null; do\n\
  sleep 1\n\
done\n\
echo "RabbitMQ is ready"\n\
\n\
echo "Starting WebApp..."\n\
exec dotnet WebApp.dll\n\
' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

# Expose port 8080 (default for non-root ASP.NET containers)
EXPOSE 8080

# Set environment variables for container
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]
