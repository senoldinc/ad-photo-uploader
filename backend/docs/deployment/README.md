# AD Photo Manager Backend - Deployment Guide

## Prerequisites

- .NET 8 SDK
- SQL Server (2019 or later)
- Active Directory access
- Docker (optional, for containerized deployment)

## Configuration

### 1. Database Setup

Create a SQL Server database:

```sql
CREATE DATABASE AdPhotoManager;
```

### 2. Environment Variables

Create `appsettings.Production.json` or set environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=AdPhotoManager;User Id=your-user;Password=your-password;TrustServerCertificate=True"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-minimum-32-characters-long",
    "Issuer": "AdPhotoManager",
    "Audience": "AdPhotoManager",
    "ExpirationMinutes": 60
  },
  "ActiveDirectory": {
    "Server": "your-ad-server.com",
    "Port": 389,
    "UseSsl": false,
    "ServiceAccount": "CN=Service Account,OU=Users,DC=domain,DC=com",
    "ServicePassword": "your-service-password",
    "BaseDn": "DC=domain,DC=com",
    "AllowedOrganizations": [
      "IT Department",
      "HR Department"
    ]
  }
}
```

### 3. Run Database Migrations

```bash
cd backend/src/AdPhotoManager.Infrastructure
dotnet ef database update --startup-project ../AdPhotoManager.Api
```

## Deployment Options

### Option 1: Direct Deployment (IIS/Kestrel)

1. Build the application:
```bash
cd backend
dotnet publish src/AdPhotoManager.Api/AdPhotoManager.Api.csproj -c Release -o ./publish
```

2. Copy `publish` folder to your server

3. Run with Kestrel:
```bash
cd publish
dotnet AdPhotoManager.Api.dll
```

4. Or configure IIS to host the application

### Option 2: Docker Deployment

1. Build Docker image:
```bash
cd backend
docker build -t adphotomanager-api:latest .
```

2. Run with docker-compose:
```bash
docker-compose up -d
```

3. Check health:
```bash
curl http://localhost:5000/health
```

### Option 3: Azure App Service

1. Create Azure App Service (Linux, .NET 8)

2. Configure Application Settings in Azure Portal

3. Deploy using Azure CLI:
```bash
az webapp up --name your-app-name --resource-group your-rg --runtime "DOTNETCORE:8.0"
```

## Post-Deployment

### 1. Verify Health Checks

```bash
curl https://your-domain/health
curl https://your-domain/health/ready
```

### 2. Test API

Access Swagger UI: `https://your-domain/swagger`

### 3. Configure Hangfire (Optional)

If using existing Hangfire infrastructure, register the sync job:

```csharp
RecurringJob.AddOrUpdate<AdSyncJob>(
    "ad-user-sync",
    job => job.ExecuteAsync(CancellationToken.None),
    "0 */6 * * *"); // Every 6 hours
```

## Monitoring

- **Logs**: Check `logs/` directory for Serilog output
- **Health**: Monitor `/health` and `/health/ready` endpoints
- **Metrics**: Integrate with Application Insights (optional)

## Security Checklist

- ✅ HTTPS enabled in production
- ✅ JWT secret key is strong (32+ characters)
- ✅ AD service account has minimal permissions
- ✅ Database credentials are secure
- ✅ CORS configured for frontend origin only
- ✅ Rate limiting enabled
- ✅ Security headers configured

## Troubleshooting

### Database Connection Issues
- Verify connection string
- Check SQL Server allows remote connections
- Ensure firewall rules allow traffic

### AD Connection Issues
- Test LDAP connectivity: `telnet ad-server 389`
- Verify service account credentials
- Check AD server is reachable from deployment environment

### Photo Upload Issues
- Verify thumbnailPhoto attribute permissions
- Check image size limits (500KB source, 100KB output)
- Review logs for detailed error messages
