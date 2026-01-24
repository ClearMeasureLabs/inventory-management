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

# Copy config files to the location expected by the app
COPY environments/global.config.json Presentation/WebApp/config/global.config.json
COPY environments/local/local.config.json Presentation/WebApp/config/local.config.json

# Build and publish
RUN dotnet publish Presentation/WebApp/WebApp.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Expose port 8080 (default for non-root ASP.NET containers)
EXPOSE 8080

# Set environment variables for container
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "WebApp.dll"]
