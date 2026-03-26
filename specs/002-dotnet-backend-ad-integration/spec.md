# Feature Specification: .NET 8 Backend with Active Directory Integration

## Overview

Production-ready .NET 8 Web API backend for AD Photo Manager application that integrates with Active Directory for authentication, user synchronization, and photo management.

## Core Requirements

### 1. Active Directory Integration

**Authentication & Authorization**
- OIDC/OAuth2 authentication using Azure AD or on-premise AD FS
- JWT token-based authentication for API endpoints
- Role-based access control (RBAC)
- Only users from specific AD organizations can access the system

**User Synchronization**
- Automated sync of user data from Active Directory
- Sync user attributes: displayName, employeeId, title, organization, department, email
- Scheduled background job for periodic sync (configurable interval)
- Manual sync trigger endpoint for administrators
- Organization-based filtering during sync (only specific OUs)

### 2. API Endpoints

**Authentication**
- `POST /api/auth/login` - OIDC login initiation
- `POST /api/auth/callback` - OIDC callback handler
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/logout` - Logout and token invalidation

**User Management**
- `GET /api/users` - List users with pagination, search, and filtering
  - Query params: search (name/employeeId/department), organization, page, pageSize
- `GET /api/users/{id}` - Get user details
- `POST /api/users/sync` - Trigger manual AD sync (admin only)

**Photo Management**
- `POST /api/users/{id}/photo` - Upload user photo
  - Validates: JPEG format, max 500KB source, 100KB output after processing
  - Circular crop to 300×300px
  - Uploads to Active Directory thumbnailPhoto attribute
- `GET /api/users/{id}/photo` - Retrieve user photo from AD
- `DELETE /api/users/{id}/photo` - Remove user photo from AD

### 3. Data Model

**User Entity (synced from AD)**
```csharp
public class User
{
    public Guid Id { get; set; }
    public string ObjectId { get; set; } // AD ObjectGUID
    public string DisplayName { get; set; }
    public string EmployeeId { get; set; }
    public string Title { get; set; }
    public string Organization { get; set; }
    public string Department { get; set; }
    public string Email { get; set; }
    public bool HasPhoto { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 4. Technical Requirements

**Framework & Libraries**
- .NET 8 Web API
- Entity Framework Core 8 for data persistence
- Microsoft.Identity.Web for OIDC/Azure AD integration
- System.DirectoryServices.Protocols for AD LDAP operations
- Hangfire or Quartz.NET for background jobs
- Serilog for structured logging

**Database**
- SQL Server or PostgreSQL for user data cache
- Store synced user data locally for performance
- Track sync status and timestamps

**Security**
- HTTPS only (enforce in production)
- JWT token validation on all protected endpoints
- Organization-based authorization filter
- Input validation and sanitization
- Rate limiting on photo upload endpoints
- Secure credential storage (Azure Key Vault or environment variables)

**Performance**
- Response time: <200ms for user listing (p95)
- Photo upload: <2s end-to-end (p95)
- Support 100+ concurrent users
- Efficient AD queries with pagination

**Configuration**
- appsettings.json for environment-specific settings
- Environment variables for sensitive data
- Configurable AD connection settings:
  - LDAP server URL
  - Base DN for user search
  - Organization filter (OU paths)
  - Sync interval
  - Service account credentials

### 5. Error Handling

- Structured error responses with Turkish messages
- Logging of all AD operations
- Graceful degradation if AD is unavailable
- Retry logic for transient AD failures
- Detailed error messages for debugging (non-production only)

### 6. Deployment

- Docker container support
- Health check endpoints (`/health`, `/health/ready`)
- Prometheus metrics endpoint
- Environment-based configuration (Development, Staging, Production)
- CI/CD pipeline ready (GitHub Actions or Azure DevOps)

## Success Criteria

1. ✅ Users can authenticate using their AD credentials
2. ✅ Only users from specified organizations can access the API
3. ✅ User data syncs automatically from AD every N hours
4. ✅ Photos upload successfully to AD thumbnailPhoto attribute
5. ✅ API responds within performance targets
6. ✅ All endpoints have proper error handling and logging
7. ✅ Production deployment is secure and scalable

## Out of Scope

- User profile editing (read-only from AD)
- Multi-tenant support
- Photo editing/filters beyond circular crop
- Mobile app backend (future consideration)
- Audit trail/history (future consideration)

## Turkish Language Support

All API error messages and user-facing text must be in Turkish to match the frontend application.
