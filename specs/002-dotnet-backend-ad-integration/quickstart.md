# Quickstart Guide: .NET 8 Backend with Active Directory Integration

**Version**: 1.0.0
**Last Updated**: 2026-03-26

---

## Prerequisites

- .NET 8 SDK
- SQL Server (existing database)
- Active Directory access (LDAP credentials)
- Azure AD or AD FS for OIDC (optional for local dev)
- Existing Hangfire project (for background jobs)

---

## 1. Clone and Setup

```bash
# Clone repository
git clone <repository-url>
cd ad-photo-uploader

# Navigate to backend
cd backend

# Restore dependencies
dotnet restore
```

---

## 2. Configuration

### appsettings.Development.json

Create `src/AdPhotoManager.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-sql-server;Database=AdPhotoManager;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "ActiveDirectory": {
    "Server": "ldap://your-ad-server.local",
    "Port": 389,
    "UseSsl": false,
    "BaseDn": "DC=example,DC=local",
    "ServiceAccount": "CN=ServiceAccount,OU=ServiceAccounts,DC=example,DC=local",
    "ServicePassword": "your-service-password",
    "AllowedOrganizations": [
      "OU=IT,DC=example,DC=local",
      "OU=Engineering,DC=example,DC=local"
    ],
    "SyncIntervalHours": 4
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "CallbackPath": "/signin-oidc"
  },
  "Jwt": {
    "Issuer": "https://localhost:5001",
    "Audience": "ad-photo-manager",
    "SecretKey": "your-secret-key-min-32-chars-long",
    "ExpirationMinutes": 60
  },
  "Hangfire": {
    "ConnectionString": "Server=your-hangfire-server;Database=HangfireDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment Variables (Alternative)

```bash
export ConnectionStrings__DefaultConnection="Server=your-sql-server;Database=AdPhotoManager;User Id=sa;Password=***;TrustServerCertificate=True"
export ActiveDirectory__Server="ldap://your-ad-server.local"
export ActiveDirectory__ServiceAccount="CN=ServiceAccount,OU=ServiceAccounts,DC=example,DC=local"
export ActiveDirectory__ServicePassword="your-service-password"
export AzureAd__TenantId="your-tenant-id"
export AzureAd__ClientId="your-client-id"
export AzureAd__ClientSecret="your-client-secret"
export Hangfire__ConnectionString="Server=your-hangfire-server;Database=HangfireDb;Trusted_Connection=True"
```

---

## 3. Database Setup

### Connect to Existing SQL Server

The application uses your existing SQL Server database. Update the connection string in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-sql-server;Database=AdPhotoManager;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**For SQL Authentication:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-sql-server;Database=AdPhotoManager;User Id=your-user;Password=your-password;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Create Database (if needed)

If the database doesn't exist yet:

```sql
-- Connect to SQL Server and run:
CREATE DATABASE AdPhotoManager;
GO
```

### Run Migrations

```bash
cd src/AdPhotoManager.Api

# Create initial migration
dotnet ef migrations add InitialCreate --project ../AdPhotoManager.Infrastructure

# Apply migrations to your SQL Server
dotnet ef database update --project ../AdPhotoManager.Infrastructure
```

---

## 4. Run the Application

### Development Mode

```bash
cd src/AdPhotoManager.Api

# Run with hot reload
dotnet watch run

# Or standard run
dotnet run
```

Application will start at:
- API: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger
- Hangfire Dashboard: (Access via your existing Hangfire project)

### Production Mode

```bash
# Build
dotnet publish -c Release -o ./publish

# Run
cd publish
dotnet AdPhotoManager.Api.dll
```

---

## 5. Hangfire Integration

### Configure Hangfire Client

The API will use your existing Hangfire project for background jobs. Configure the Hangfire client in `Program.cs`:

```csharp
// Add Hangfire client (not server - that's in your existing Hangfire project)
builder.Services.AddHangfire(config => config
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

// Add Hangfire client only (no server)
builder.Services.AddHangfireClient();
```

### Register AD Sync Job

In your existing Hangfire project, register the AD sync job:

```csharp
// In your existing Hangfire project's startup
RecurringJob.AddOrUpdate<AdSyncJob>(
    "ad-photo-sync",
    job => job.ExecuteAsync(),
    "0 */4 * * *", // Every 4 hours
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });
```

### Manual Sync Trigger

The API can trigger manual syncs by enqueueing jobs to your existing Hangfire:

```csharp
// In API controller
BackgroundJob.Enqueue<AdSyncJob>(job => job.ExecuteAsync());
```

---

## 6. Docker Deployment (Optional)

If you want to containerize the API (database and Hangfire remain external):

### Build Image

```bash
cd backend

# Build Docker image
docker build -t adphoto-backend:latest -f docker/Dockerfile .
```

**docker/Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AdPhotoManager.Api/AdPhotoManager.Api.csproj", "AdPhotoManager.Api/"]
COPY ["src/AdPhotoManager.Core/AdPhotoManager.Core.csproj", "AdPhotoManager.Core/"]
COPY ["src/AdPhotoManager.Infrastructure/AdPhotoManager.Infrastructure.csproj", "AdPhotoManager.Infrastructure/"]
COPY ["src/AdPhotoManager.Shared/AdPhotoManager.Shared.csproj", "AdPhotoManager.Shared/"]
RUN dotnet restore "AdPhotoManager.Api/AdPhotoManager.Api.csproj"
COPY src/ .
WORKDIR "/src/AdPhotoManager.Api"
RUN dotnet build "AdPhotoManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdPhotoManager.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdPhotoManager.Api.dll"]
```

### Run Container

```bash
docker run -d \
  -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="Server=your-sql-server;Database=AdPhotoManager;User Id=sa;Password=***" \
  -e ActiveDirectory__Server="ldap://your-ad-server.local" \
  -e ActiveDirectory__ServiceAccount="CN=ServiceAccount,OU=ServiceAccounts,DC=example,DC=local" \
  -e ActiveDirectory__ServicePassword="your-password" \
  -e Hangfire__ConnectionString="Server=your-hangfire-server;Database=HangfireDb;User Id=sa;Password=***" \
  --name adphoto-api \
  adphoto-backend:latest
```

---

## 6. Testing

### Run Unit Tests

```bash
cd tests/AdPhotoManager.Core.Tests
dotnet test
```

### Run Integration Tests

```bash
# Requires Docker for Testcontainers (OpenLDAP)
cd tests/AdPhotoManager.Infrastructure.Tests
dotnet test
```

### Run All Tests

```bash
cd backend
dotnet test
```

---

## 7. API Usage Examples

### Run Unit Tests

```bash
cd tests/AdPhotoManager.Core.Tests
dotnet test
```

### Run Integration Tests

```bash
# Requires Docker for Testcontainers
cd tests/AdPhotoManager.Infrastructure.Tests
dotnet test
```

### Run All Tests

```bash
cd backend
dotnet test
```

---

## 7. API Usage Examples

### Authentication

```bash
# Login (get redirect URL)
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"returnUrl": "/dashboard"}'

# After OIDC flow, exchange code for token
curl -X POST https://localhost:5001/api/auth/callback \
  -H "Content-Type: application/json" \
  -d '{"code": "auth-code", "state": "state-token"}'
```

### List Users

```bash
curl -X GET "https://localhost:5001/api/users?search=ahmet&page=1&pageSize=20" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Upload Photo

```bash
curl -X POST "https://localhost:5001/api/users/USER_ID/photo" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "photo=@/path/to/photo.jpg" \
  -F "quality=85"
```

### Trigger Manual Sync

```bash
curl -X POST "https://localhost:5001/api/users/sync" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"fullSync": true}'
```

---

## 8. Monitoring

### Hangfire Dashboard

Access your existing Hangfire dashboard to monitor AD sync jobs:
- View scheduled jobs
- Monitor sync status
- Retry failed jobs
- View job history

### Health Checks

```bash
# Liveness check
curl https://localhost:5001/health

# Readiness check
curl https://localhost:5001/health/ready
```

### Logs

```bash
# Docker logs (if containerized)
docker logs -f adphoto-api

# Local development
# Logs written to console and file (logs/app.log)
```

---

## 9. Troubleshooting

### Database Connection Issues

```bash
# Test SQL Server connection
sqlcmd -S your-sql-server -d AdPhotoManager -E

# Or with SQL authentication
sqlcmd -S your-sql-server -d AdPhotoManager -U your-user -P your-password

# Check if database exists
sqlcmd -S your-sql-server -Q "SELECT name FROM sys.databases WHERE name = 'AdPhotoManager'"
```

### Active Directory Connection Issues

```bash
# Test LDAP connection
ldapsearch -x -H ldap://your-ad-server.local -D "CN=ServiceAccount,OU=ServiceAccounts,DC=example,DC=local" -W -b "DC=example,DC=local"
```

### Migration Issues

```bash
# Drop database and recreate (WARNING: deletes all data)
dotnet ef database drop --force
dotnet ef database update

# Or manually in SQL Server
# DROP DATABASE AdPhotoManager;
# CREATE DATABASE AdPhotoManager;
# dotnet ef database update
```

### Port Already in Use

```bash
# Change port in launchSettings.json or use environment variable
export ASPNETCORE_URLS="https://localhost:5002"
dotnet run
```

### Hangfire Connection Issues

If the API cannot connect to your existing Hangfire database:
- Verify the Hangfire connection string in appsettings.json
- Ensure the Hangfire database is accessible from the API server
- Check firewall rules between API and Hangfire SQL Server

---

## 10. Next Steps

1. **Configure Database Connection**: Update connection string to point to your SQL Server
2. **Configure AD Organizations**: Update `AllowedOrganizations` in appsettings.json
3. **Set Up OIDC**: Configure Azure AD or AD FS application registration
4. **Integrate with Existing Hangfire**: Register AD sync job in your Hangfire project
5. **Customize Sync Schedule**: Adjust sync interval in Hangfire recurring job configuration
6. **Enable HTTPS**: Configure SSL certificate for production
7. **Set Up Monitoring**: Integrate with Application Insights or your existing monitoring solution
8. **Configure CORS**: Add frontend URL to CORS policy

---

## Additional Resources

- [API Documentation](./contracts/) - Detailed API contracts
- [Data Model](./data-model.md) - Database schema and entities
- [Research](./research.md) - Technology decisions and rationale
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Hangfire Documentation](https://docs.hangfire.io/)

---

## Architecture Notes

### Using Existing Infrastructure

This backend is designed to integrate with your existing infrastructure:

**SQL Server Database**:
- Uses your existing SQL Server instance
- No Docker or PostgreSQL setup required
- Standard EF Core migrations for schema management

**Hangfire Integration**:
- Connects to your existing Hangfire project
- API acts as Hangfire client (not server)
- Background jobs run in your existing Hangfire infrastructure
- Shared monitoring through existing Hangfire dashboard

**Benefits**:
- No duplicate infrastructure
- Consistent operational procedures
- Leverages existing monitoring and alerting
- Simplified deployment and maintenance
