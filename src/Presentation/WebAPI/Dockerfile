FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files
COPY ["Core/Domain/Domain.csproj", "Core/Domain/"]
COPY ["Core/Application/Application.csproj", "Core/Application/"]
COPY ["Infrastructure/Bootstrap/Bootstrap.csproj", "Infrastructure/Bootstrap/"]
COPY ["Infrastructure/Redis/Redis.csproj", "Infrastructure/Redis/"]
COPY ["Infrastructure/RabbitMQ/RabbitMQ.csproj", "Infrastructure/RabbitMQ/"]
COPY ["Infrastructure/SQLServer/SQLServer.csproj", "Infrastructure/SQLServer/"]
COPY ["Presentation/WebAPI/WebAPI.csproj", "Presentation/WebAPI/"]

# Restore
RUN dotnet restore "Presentation/WebAPI/WebAPI.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/Presentation/WebAPI"
RUN dotnet build "WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy config files
COPY ["environments/local/local.config.json", "config/local.config.json"]
COPY ["environments/global.config.json", "config/global.config.json"]

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "WebAPI.dll"]
