# Research: .NET 8 Backend with Active Directory Integration

**Date**: 2026-03-26
**Feature**: 002-dotnet-backend-ad-integration

## Overview

This document consolidates research findings for key technical decisions in the .NET 8 backend implementation.

---

## 1. Database Selection: SQL Server (Existing)

### Decision: **SQL Server (Existing Database)**

### Rationale

Using existing SQL Server database infrastructure:

**Primary reasons:**
- **Existing infrastructure**: Database already provisioned and managed
- **No additional setup**: Eliminates Docker/PostgreSQL setup complexity
- **Team familiarity**: Existing SQL Server expertise and tooling
- **EF Core support**: Entity Framework Core 8 has excellent SQL Server provider
- **Enterprise features**: Advanced security, backup, and monitoring already in place

**Implementation:**
```csharp
// NuGet packages
- Microsoft.EntityFrameworkCore.SqlServer (8.x)
- Microsoft.EntityFrameworkCore.Design

// Connection string
"Server=your-server;Database=AdPhotoManager;Trusted_Connection=True;TrustServerCertificate=True"
// Or with SQL authentication:
"Server=your-server;Database=AdPhotoManager;User Id=sa;Password=***;TrustServerCertificate=True"
```

### Alternatives Considered

**PostgreSQL** - Not needed:
- Would require additional Docker infrastructure
- Team already has SQL Server expertise
- No compelling reason to introduce new database technology

**SQLite** - Not suitable:
- Existing enterprise infrastructure already in place
- SQL Server provides better scalability and features

---

## 2. Background Job Scheduler: Hangfire (Existing)

### Decision: **Hangfire (Existing Project)**

### Rationale

Using existing Hangfire infrastructure:

**Primary reasons:**
- **Already deployed**: Hangfire project already exists and running
- **Proven reliability**: Battle-tested in current environment
- **Shared infrastructure**: Can leverage existing monitoring and alerting
- **No duplication**: Avoid running multiple job schedulers
- **Consistent operations**: Same operational procedures as other jobs

**Integration approach:**
- Register AD sync job in existing Hangfire project
- Use Hangfire client to enqueue jobs from API
- Share connection string with existing Hangfire database

**Implementation:**
```csharp
// In API project - enqueue job to existing Hangfire
services.AddHangfireClient(config => config
    .UseSqlServerStorage(hangfireConnectionString));

// Trigger sync from API
BackgroundJob.Enqueue<AdSyncJob>(job => job.ExecuteAsync());

// Or schedule recurring job
RecurringJob.AddOrUpdate<AdSyncJob>(
    "ad-sync",
    job => job.ExecuteAsync(),
    "0 */4 * * *"); // Every 4 hours
```

### Alternatives Considered

**Standalone Hangfire in API** - Not needed:
- Would duplicate existing infrastructure
- Increases operational complexity
- Existing Hangfire project can handle AD sync jobs

**Quartz.NET** - Not needed:
- Existing Hangfire infrastructure already proven
- No reason to introduce different scheduler

---

## 3. Active Directory Testing Strategy

### Decision: **Abstraction Layer + Mocking (Unit) + Testcontainers (Integration)**

### Rationale

Balanced approach for testing AD/LDAP operations:

**Unit Testing (70-80% of tests):**
- Create `ILdapConnection` abstraction over `System.DirectoryServices.Protocols`
- Mock with Moq/NSubstitute for fast, isolated tests
- Test business logic independently

**Integration Testing (20-30% of tests):**
- Use Testcontainers with OpenLDAP Docker image
- Test actual LDAP operations on critical paths
- Verify real AD behavior

**Implementation:**

```csharp
// Abstraction layer
public interface ILdapConnection
{
    SearchResponse Search(SearchRequest request);
    ModifyResponse Modify(ModifyRequest request);
    AddResponse Add(AddRequest request);
}

// Unit test example
[Fact]
public async Task GetUserPhoto_ReturnsPhoto_WhenUserExists()
{
    var mockConnection = new Mock<ILdapConnection>();
    mockConnection
        .Setup(x => x.Search(It.IsAny<SearchRequest>()))
        .Returns(CreateSearchResponseWithPhoto(expectedPhoto));

    var service = new AdPhotoService(mockConnection.Object);
    var result = await service.GetUserPhotoAsync("user@domain.com");

    Assert.Equal(expectedPhoto, result);
}

// Integration test with Testcontainers
public class AdIntegrationTests : IAsyncLifetime
{
    private IContainer _ldapContainer;

    public async Task InitializeAsync()
    {
        _ldapContainer = new ContainerBuilder()
            .WithImage("osixia/openldap:latest")
            .WithPortBinding(389, 389)
            .Build();
        await _ldapContainer.StartAsync();
    }

    [Fact]
    public async Task UploadPhoto_StoresInLdap_Successfully()
    {
        var service = new AdPhotoService(_connection);
        await service.UploadPhotoAsync("testuser", photo);

        var retrieved = await service.GetUserPhotoAsync("testuser");
        Assert.Equal(photo, retrieved);
    }
}
```

**Tools:**
- **Mocking**: Moq (most popular, fluent API)
- **Test Containers**: Testcontainers for .NET
- **LDAP Server**: OpenLDAP (Docker: `osixia/openldap`)
- **Testing Framework**: xUnit (recommended for .NET 8)

### Alternatives Considered

**Mock-only approach:**
- Pros: Very fast, no Docker dependency
- Cons: Low realism, doesn't catch LDAP-specific issues

**Test AD instance only:**
- Pros: Highest realism
- Cons: Slow, complex setup, requires infrastructure

---

## 4. Additional Technology Decisions

### Image Processing: **SixLabors.ImageSharp**

**Rationale:**
- Cross-platform, pure .NET implementation
- Excellent performance for resize/crop operations
- Active maintenance and .NET 8 support
- Circular crop with transparency support

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

public byte[] CreateCircularCrop(byte[] sourceImage, int size = 300)
{
    using var image = Image.Load(sourceImage);
    image.Mutate(x => x
        .Resize(size, size)
        .ApplyCircularMask());

    using var ms = new MemoryStream();
    image.SaveAsJpeg(ms, new JpegEncoder { Quality = 85 });
    return ms.ToArray();
}
```

### Authentication: **Microsoft.Identity.Web**

**Rationale:**
- Official Microsoft library for OIDC/Azure AD
- Seamless integration with ASP.NET Core 8
- Supports both Azure AD and on-premise AD FS
- Built-in JWT validation and token management

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
```

### Logging: **Serilog**

**Rationale:**
- Structured logging with rich context
- Multiple sinks (Console, File, Seq, Application Insights)
- Excellent performance
- Easy integration with ASP.NET Core

```csharp
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));
```

---

## Summary

All technical decisions resolved:

| Component | Decision | Key Benefit |
|-----------|----------|-------------|
| Database | SQL Server (existing) | No setup needed, existing infrastructure |
| Job Scheduler | Hangfire (existing project) | Shared infrastructure, proven reliability |
| Testing | Abstraction + Mocking + Testcontainers | Fast unit tests, realistic integration tests |
| Image Processing | SixLabors.ImageSharp | Cross-platform, excellent performance |
| Authentication | Microsoft.Identity.Web | Official OIDC/Azure AD support |
| Logging | Serilog | Structured logging, multiple sinks |

**Next Steps**: Proceed to Phase 1 (data model, contracts, quickstart documentation)
