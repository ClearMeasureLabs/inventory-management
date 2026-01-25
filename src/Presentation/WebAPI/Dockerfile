FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files
COPY ["src/Core/Domain/Domain.csproj", "Core/Domain/"]
COPY ["src/Core/Application/Application.csproj", "Core/Application/"]
COPY ["src/Infrastructure/Bootstrap/Bootstrap.csproj", "Infrastructure/Bootstrap/"]
COPY ["src/Infrastructure/Redis/Redis.csproj", "Infrastructure/Redis/"]
COPY ["src/Infrastructure/RabbitMQ/RabbitMQ.csproj", "Infrastructure/RabbitMQ/"]
COPY ["src/Infrastructure/SQLServer/SQLServer.csproj", "Infrastructure/SQLServer/"]
COPY ["src/Presentation/WebAPI/WebAPI.csproj", "Presentation/WebAPI/"]

# Copy environments folder (needed for project references)
COPY ["environments/", "/environments/"]

# Restore
RUN dotnet restore "Presentation/WebAPI/WebAPI.csproj"

# Copy source directories
COPY src/Core/Domain/ Core/Domain/
COPY src/Core/Application/ Core/Application/
COPY src/Infrastructure/Bootstrap/ Infrastructure/Bootstrap/
COPY src/Infrastructure/Redis/ Infrastructure/Redis/
COPY src/Infrastructure/RabbitMQ/ Infrastructure/RabbitMQ/
COPY src/Infrastructure/SQLServer/ Infrastructure/SQLServer/
COPY src/Presentation/WebAPI/ Presentation/WebAPI/

# Build
WORKDIR "/src/Presentation/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "WebAPI.dll"]
