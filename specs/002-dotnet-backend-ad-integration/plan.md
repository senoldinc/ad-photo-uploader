# Implementation Plan: .NET 8 Backend with Active Directory Integration

**Branch**: `002-dotnet-backend-ad-integration` | **Date**: 2026-03-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-dotnet-backend-ad-integration/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

Production-ready .NET 8 Web API backend that integrates with Active Directory for authentication, user synchronization, and photo management. The system authenticates users via OIDC, syncs user data from specific AD organizations, and enables uploading circular-cropped photos (300×300px, ≤100KB) directly to AD's thumbnailPhoto attribute.

## Technical Context

**Language/Version**: .NET 8 (C# 12)
**Primary Dependencies**:
- ASP.NET Core 8.0 (Web API)
- Entity Framework Core 8.0 (ORM)
- Microsoft.EntityFrameworkCore.SqlServer 8.x (SQL Server provider)
- Microsoft.Identity.Web (OIDC/Azure AD authentication)
- System.DirectoryServices.Protocols (LDAP/AD operations)
- Hangfire (existing project - background job scheduling)
- Serilog (structured logging)
- SixLabors.ImageSharp (image processing for circular crop)

**Storage**:
- SQL Server (existing database - no Docker setup needed)
- Active Directory (LDAP) as source of truth for user data
- AD thumbnailPhoto attribute for photo storage

**Testing**:
- xUnit for unit and integration tests
- Moq for mocking (abstraction layer over LDAP)
- Testcontainers for .NET with OpenLDAP (integration tests)

**Target Platform**:
- Windows/Linux Server (IIS, Kestrel, or Docker)
- Integrates with existing SQL Server and Hangfire infrastructure
- Flexible deployment: on-premise, Azure App Service, or containerized

**Project Type**: Web service (REST API)

**Performance Goals**:
- User listing: <200ms response time (p95)
- Photo upload: <2s end-to-end (p95)
- Support 100+ concurrent users
- AD sync: process 1000+ users within 5 minutes

**Constraints**:
- HTTPS only in production
- JWT token expiration: 1 hour (configurable)
- Photo size: max 500KB input, 100KB output
- Organization-based access control (only specific AD OUs)
- Turkish language for all error messages

**Scale/Scope**:
- Expected users: 100-1000 AD users
- Filtered organizations: 1-5 AD organizational units
- API endpoints: ~15 endpoints
- Background jobs: 1-2 scheduled tasks

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Status**: ✅ PASSED (Post-Phase 1 Evaluation)

**Recommended Gates for Production-Ready Backend**:
- ✅ Test-first approach: Unit tests for business logic, integration tests for AD operations
  - **Design**: Abstraction layer (ILdapConnection) enables mocking for unit tests
  - **Design**: Testcontainers with OpenLDAP for realistic integration tests
  - **Coverage**: 70-80% unit tests, 20-30% integration tests

- ✅ Security: HTTPS only, JWT validation, input sanitization, secure credential storage
  - **Design**: Microsoft.Identity.Web for OIDC/JWT authentication
  - **Design**: Organization-based authorization filter
  - **Design**: Input validation with FluentValidation
  - **Design**: Environment variables for sensitive data

- ✅ Observability: Structured logging (Serilog), health checks, metrics endpoint
  - **Design**: Serilog with multiple sinks (Console, File, Application Insights)
  - **Design**: Health check endpoints (/health, /health/ready)
  - **Design**: Hangfire dashboard for job monitoring

- ✅ Error handling: Graceful degradation, retry logic, Turkish error messages
  - **Design**: Structured error responses with Turkish messages
  - **Design**: Retry logic for transient AD failures (Polly)
  - **Design**: Detailed logging of all AD operations

- ✅ Documentation: API contracts (OpenAPI/Swagger), deployment guide, configuration guide
  - **Design**: Complete API contracts in /contracts/ directory
  - **Design**: Quickstart guide with Docker deployment
  - **Design**: Configuration examples for all environments

**Architecture Compliance**:
- ✅ Clean Architecture: Clear separation (Api, Core, Infrastructure, Shared)
- ✅ Dependency Inversion: Core has no infrastructure dependencies
- ✅ Single Responsibility: Each layer has focused purpose
- ✅ Testability: Abstraction layers enable comprehensive testing

**No violations** - Design adheres to production-ready standards.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── AdPhotoManager.Api/              # Web API project
│   │   ├── Controllers/                 # API endpoints
│   │   ├── Middleware/                  # Auth, error handling, logging
│   │   ├── Program.cs                   # Application entry point
│   │   └── appsettings.json            # Configuration
│   │
│   ├── AdPhotoManager.Core/             # Domain layer
│   │   ├── Entities/                    # Domain models (User)
│   │   ├── Interfaces/                  # Repository & service contracts
│   │   ├── Services/                    # Business logic
│   │   └── Exceptions/                  # Custom exceptions
│   │
│   ├── AdPhotoManager.Infrastructure/   # Data access & external services
│   │   ├── Data/                        # EF Core DbContext, migrations
│   │   ├── Repositories/                # Repository implementations
│   │   ├── ActiveDirectory/             # AD integration (LDAP operations)
│   │   ├── BackgroundJobs/              # Scheduled sync jobs
│   │   └── ImageProcessing/             # Photo crop & resize logic
│   │
│   └── AdPhotoManager.Shared/           # Shared DTOs, constants, utilities
│       ├── DTOs/                        # Request/response models
│       ├── Constants/                   # Error codes, config keys
│       └── Extensions/                  # Helper extensions
│
├── tests/
│   ├── AdPhotoManager.Api.Tests/        # API integration tests
│   ├── AdPhotoManager.Core.Tests/       # Unit tests for business logic
│   └── AdPhotoManager.Infrastructure.Tests/  # Integration tests (DB, AD mocks)
│
├── docker/
│   ├── Dockerfile                       # Production container
│   └── docker-compose.yml               # Local development stack
│
└── docs/
    ├── api/                             # OpenAPI/Swagger specs
    ├── deployment/                      # Deployment guides
    └── configuration/                   # Configuration reference

frontend/                                # Existing React app (unchanged)
```

**Structure Decision**: Clean Architecture pattern with three layers:
- **Api**: Presentation layer (controllers, middleware, startup)
- **Core**: Domain layer (entities, interfaces, business logic) - no external dependencies
- **Infrastructure**: Implementation layer (EF Core, AD integration, background jobs)
- **Shared**: Cross-cutting concerns (DTOs, constants, extensions)

This structure ensures:
- Clear separation of concerns
- Testability (Core has no infrastructure dependencies)
- Maintainability (each layer has single responsibility)
- Standard .NET project organization

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
