# Multi-stage build for .NET 8 API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/AdPhotoManager.Api/AdPhotoManager.Api.csproj", "src/AdPhotoManager.Api/"]
COPY ["src/AdPhotoManager.Core/AdPhotoManager.Core.csproj", "src/AdPhotoManager.Core/"]
COPY ["src/AdPhotoManager.Infrastructure/AdPhotoManager.Infrastructure.csproj", "src/AdPhotoManager.Infrastructure/"]
COPY ["src/AdPhotoManager.Shared/AdPhotoManager.Shared.csproj", "src/AdPhotoManager.Shared/"]

# Restore dependencies
RUN dotnet restore "src/AdPhotoManager.Api/AdPhotoManager.Api.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/src/AdPhotoManager.Api"
RUN dotnet build "AdPhotoManager.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "AdPhotoManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "AdPhotoManager.Api.dll"]
