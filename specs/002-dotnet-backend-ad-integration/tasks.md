# Implementation Tasks: .NET 8 Backend with Active Directory Integration

**Feature**: 002-dotnet-backend-ad-integration
**Branch**: `002-dotnet-backend-ad-integration`
**Generated**: 2026-03-26

---

## Overview

This document provides a complete, dependency-ordered task list for implementing the .NET 8 backend with Active Directory integration. Tasks are organized by user story to enable independent implementation and testing.

**Total Tasks**: 67
**Parallelizable Tasks**: 28
**User Stories**: 4

---

## Implementation Strategy

**MVP Scope** (Minimum Viable Product):
- Phase 1: Setup
- Phase 2: Foundational
- Phase 3: US1 - Authentication & Authorization
- Phase 4: US2 - User Synchronization

**Incremental Delivery**:
- After MVP: US3 - User Management API
- Final: US4 - Photo Management
- Polish: Cross-cutting concerns

---

## Phase 1: Setup & Project Structure

**Goal**: Initialize .NET 8 solution with Clean Architecture structure

**Tasks**:

- [X] T001 Create backend directory and .NET 8 solution file in backend/AdPhotoManager.sln
- [X] T002 Create AdPhotoManager.Core class library project in backend/src/AdPhotoManager.Core/
- [X] T003 Create AdPhotoManager.Infrastructure class library project in backend/src/AdPhotoManager.Infrastructure/
- [X] T004 Create AdPhotoManager.Shared class library project in backend/src/AdPhotoManager.Shared/
- [X] T005 Create AdPhotoManager.Api web API project in backend/src/AdPhotoManager.Api/
- [X] T006 Create test projects: AdPhotoManager.Core.Tests, AdPhotoManager.Infrastructure.Tests, AdPhotoManager.Api.Tests in backend/tests/
- [X] T007 Add project references: Api → Core, Infrastructure, Shared; Infrastructure → Core, Shared
- [X] T008 Install NuGet packages in Core: no external dependencies (pure domain)
- [X] T009 Install NuGet packages in Infrastructure: Microsoft.EntityFrameworkCore.SqlServer (8.x), System.DirectoryServices.Protocols, SixLabors.ImageSharp, Polly
- [X] T010 Install NuGet packages in Api: Microsoft.Identity.Web, Serilog.AspNetCore, Hangfire.AspNetCore, Swashbuckle.AspNetCore
- [ ] T011 Install NuGet packages in test projects: xUnit, Moq, FluentAssertions, Testcontainers
- [X] T012 Create directory structure in Core: Entities/, Interfaces/, Services/, Exceptions/
- [X] T013 Create directory structure in Infrastructure: Data/, Repositories/, ActiveDirectory/, BackgroundJobs/, ImageProcessing/
- [X] T014 Create directory structure in Shared: DTOs/, Constants/, Extensions/
- [X] T015 Create directory structure in Api: Controllers/, Middleware/
- [X] T016 Create appsettings.json and appsettings.Development.json in Api project with configuration sections
- [X] T017 Create .gitignore file in backend/ to exclude bin/, obj/, appsettings.*.json (except template)

---

## Phase 2: Foundational Layer

**Goal**: Implement core domain entities, database context, and shared infrastructure

**Independent Test Criteria**:
- ✅ User and SyncLog entities can be instantiated with valid data
- ✅ Database migrations generate correct SQL Server schema
- ✅ DbContext can connect to SQL Server and create tables

**Tasks**:

### Domain Entities

- [X] T018 [P] Create User entity in backend/src/AdPhotoManager.Core/Entities/User.cs with all properties per data-model.md
- [X] T019 [P] Create SyncLog entity in backend/src/AdPhotoManager.Core/Entities/SyncLog.cs with enums SyncStatus and SyncTriggerType
- [X] T020 [P] Create custom exceptions in backend/src/AdPhotoManager.Core/Exceptions/: AdConnectionException, UserNotFoundException, PhotoValidationException

### Database Layer

- [X] T021 Create ApplicationDbContext in backend/src/AdPhotoManager.Infrastructure/Data/ApplicationDbContext.cs with DbSet<User> and DbSet<SyncLog>
- [X] T022 Create UserConfiguration in backend/src/AdPhotoManager.Infrastructure/Data/Configurations/UserConfiguration.cs implementing IEntityTypeConfiguration<User>
- [X] T023 Create SyncLogConfiguration in backend/src/AdPhotoManager.Infrastructure/Data/Configurations/SyncLogConfiguration.cs implementing IEntityTypeConfiguration<SyncLog>
- [X] T024 Apply entity configurations in ApplicationDbContext.OnModelCreating
- [X] T025 Create initial EF Core migration: dotnet ef migrations add InitialCreate --project backend/src/AdPhotoManager.Infrastructure
- [X] T026 Verify migration generates correct SQL Server schema (UNIQUEIDENTIFIER, NVARCHAR, DATETIME2, indexes)

### Shared Infrastructure

- [X] T027 [P] Create ErrorCodes constants in backend/src/AdPhotoManager.Shared/Constants/ErrorCodes.cs with Turkish error messages
- [X] T028 [P] Create ConfigurationKeys constants in backend/src/AdPhotoManager.Shared/Constants/ConfigurationKeys.cs
- [X] T029 [P] Create ErrorResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/ErrorResponse.cs
- [X] T030 [P] Create PaginationResponse<T> DTO in backend/src/AdPhotoManager.Shared/DTOs/PaginationResponse.cs

---

## Phase 3: US1 - Authentication & Authorization

**User Story**: As a user, I want to authenticate using my AD credentials via OIDC, so that only users from authorized organizations can access the API.

**Independent Test Criteria**:
- ✅ OIDC login flow redirects to Azure AD/AD FS
- ✅ JWT tokens are issued after successful authentication
- ✅ Protected endpoints reject requests without valid JWT
- ✅ Users from unauthorized organizations are rejected

**Tasks**:

### Core Interfaces

- [X] T031 [P] [US1] Create IAuthService interface in backend/src/AdPhotoManager.Core/Interfaces/IAuthService.cs with methods: LoginAsync, CallbackAsync, RefreshTokenAsync, LogoutAsync
- [X] T032 [P] [US1] Create ITokenService interface in backend/src/AdPhotoManager.Core/Interfaces/ITokenService.cs with methods: GenerateAccessToken, GenerateRefreshToken, ValidateToken

### DTOs

- [X] T033 [P] [US1] Create LoginRequest DTO in backend/src/AdPhotoManager.Shared/DTOs/Auth/LoginRequest.cs
- [X] T034 [P] [US1] Create LoginResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/Auth/LoginResponse.cs
- [X] T035 [P] [US1] Create TokenResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/Auth/TokenResponse.cs with accessToken, refreshToken, expiresIn

### Infrastructure Implementation

- [X] T036 [US1] Create TokenService in backend/src/AdPhotoManager.Infrastructure/Services/TokenService.cs implementing ITokenService using System.IdentityModel.Tokens.Jwt
- [X] T037 [US1] Create AuthService in backend/src/AdPhotoManager.Infrastructure/Services/AuthService.cs implementing IAuthService with OIDC integration
- [X] T038 [US1] Create OrganizationAuthorizationFilter in backend/src/AdPhotoManager.Api/Middleware/OrganizationAuthorizationFilter.cs to validate user organization

### API Layer

- [X] T039 [US1] Create AuthController in backend/src/AdPhotoManager.Api/Controllers/AuthController.cs with endpoints: POST /api/auth/login, POST /api/auth/callback, POST /api/auth/refresh, POST /api/auth/logout
- [X] T040 [US1] Configure Microsoft.Identity.Web in Program.cs with Azure AD settings from appsettings.json
- [X] T041 [US1] Configure JWT authentication in Program.cs with token validation parameters
- [X] T042 [US1] Register OrganizationAuthorizationFilter as global filter in Program.cs
- [X] T043 [US1] Add [Authorize] attribute to controllers that require authentication

---

## Phase 4: US2 - User Synchronization

**User Story**: As a system administrator, I want user data to sync automatically from Active Directory, so that the local database stays up-to-date with AD changes.

**Independent Test Criteria**:
- ✅ Manual sync endpoint triggers AD user query and database update
- ✅ Scheduled background job runs at configured interval
- ✅ Only users from allowed organizations are synced
- ✅ SyncLog records are created for each sync operation
- ✅ Sync handles AD connection failures gracefully

**Tasks**:

### Core Interfaces

- [X] T044 [P] [US2] Create ILdapConnection interface in backend/src/AdPhotoManager.Core/Interfaces/ILdapConnection.cs with methods: Search, Modify, Add, Delete (abstraction over System.DirectoryServices.Protocols)
- [X] T045 [P] [US2] Create IUserSyncService interface in backend/src/AdPhotoManager.Core/Interfaces/IUserSyncService.cs with methods: SyncUsersAsync, GetSyncStatusAsync
- [X] T046 [P] [US2] Create IUserRepository interface in backend/src/AdPhotoManager.Core/Interfaces/IUserRepository.cs with methods: GetAllAsync, GetByIdAsync, GetByAdObjectIdAsync, AddAsync, UpdateAsync, DeleteAsync

### DTOs

- [X] T047 [P] [US2] Create SyncRequest DTO in backend/src/AdPhotoManager.Shared/DTOs/Sync/SyncRequest.cs with fullSync boolean
- [X] T048 [P] [US2] Create SyncStatusResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/Sync/SyncStatusResponse.cs with currentSync and lastSync details

### Infrastructure Implementation

- [X] T049 [US2] Create LdapConnectionAdapter in backend/src/AdPhotoManager.Infrastructure/ActiveDirectory/LdapConnectionAdapter.cs implementing ILdapConnection
- [X] T050 [US2] Create AdUserMapper in backend/src/AdPhotoManager.Infrastructure/ActiveDirectory/AdUserMapper.cs to map LDAP SearchResultEntry to User entity
- [X] T051 [US2] Create UserRepository in backend/src/AdPhotoManager.Infrastructure/Repositories/UserRepository.cs implementing IUserRepository with EF Core
- [X] T052 [US2] Create UserSyncService in backend/src/AdPhotoManager.Core/Services/UserSyncService.cs implementing IUserSyncService with sync logic
- [X] T053 [US2] Create AdSyncJob in backend/src/AdPhotoManager.Infrastructure/BackgroundJobs/AdSyncJob.cs with ExecuteAsync method for Hangfire
- [X] T054 [US2] Add Polly retry policy for transient AD failures in UserSyncService
- [X] T055 [US2] Implement organization filtering in LDAP query (filter by allowed OUs from configuration)

### API Layer

- [X] T056 [US2] Create UsersController in backend/src/AdPhotoManager.Api/Controllers/UsersController.cs with POST /api/users/sync endpoint (admin only)
- [X] T057 [US2] Add GET /api/users/sync/status endpoint to UsersController
- [X] T058 [US2] Configure Hangfire client in Program.cs to connect to existing Hangfire database
- [ ] T059 [US2] Register AdSyncJob with Hangfire recurring job in existing Hangfire project (manual step documented in quickstart.md)

---

## Phase 5: US3 - User Management API

**User Story**: As a user, I want to list, search, and view user details, so that I can find users and see their information.

**Independent Test Criteria**:
- ✅ GET /api/users returns paginated user list
- ✅ Search query filters by displayName, employeeId, department
- ✅ Organization and department filters work correctly
- ✅ GET /api/users/{id} returns user details
- ✅ Pagination metadata is accurate

**Tasks**:

### DTOs

- [X] T060 [P] [US3] Create UserListRequest DTO in backend/src/AdPhotoManager.Shared/DTOs/Users/UserListRequest.cs with search, organization, department, page, pageSize
- [X] T061 [P] [US3] Create UserResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/Users/UserResponse.cs mapping User entity properties
- [X] T062 [P] [US3] Create UserDetailResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/Users/UserDetailResponse.cs with additional fields

### Infrastructure Implementation

- [X] T063 [US3] Add pagination and filtering methods to UserRepository: GetPagedAsync, SearchAsync
- [X] T064 [US3] Implement search logic with EF Core LINQ queries (search across displayName, employeeId, department)

### API Layer

- [X] T065 [US3] Add GET /api/users endpoint to UsersController with pagination, search, and filtering
- [X] T066 [US3] Add GET /api/users/{id} endpoint to UsersController
- [ ] T067 [US3] Add AutoMapper or manual mapping from User entity to UserResponse/UserDetailResponse DTOs

---

## Phase 6: US4 - Photo Management

**User Story**: As a user, I want to upload, view, and delete user photos, so that AD user photos are kept up-to-date.

**Independent Test Criteria**:
- ✅ POST /api/users/{id}/photo validates JPEG format and size limits
- ✅ Photo is cropped to 300×300px circular format
- ✅ Output size is ≤100KB (quality adjusted if needed)
- ✅ Photo is uploaded to AD thumbnailPhoto attribute
- ✅ GET /api/users/{id}/photo retrieves photo from AD
- ✅ DELETE /api/users/{id}/photo removes photo from AD

**Tasks**:

### Core Interfaces

- [X] T068 [P] [US4] Create IPhotoService interface in backend/src/AdPhotoManager.Core/Interfaces/IPhotoService.cs with methods: UploadPhotoAsync, GetPhotoAsync, DeletePhotoAsync
- [X] T069 [P] [US4] Create IImageProcessor interface in backend/src/AdPhotoManager.Core/Interfaces/IImageProcessor.cs with methods: CreateCircularCrop, ValidatePhoto, AdjustQuality

### DTOs

- [X] T070 [P] [US4] Create PhotoUploadRequest DTO in backend/src/AdPhotoManager.Shared/DTOs/Photos/PhotoUploadRequest.cs with photo file and quality
- [X] T071 [P] [US4] Create PhotoUploadResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/Photos/PhotoUploadResponse.cs with upload status and metadata
- [X] T072 [P] [US4] Create PhotoResponse DTO in backend/src/AdPhotoManager.Shared/DTOs/Photos/PhotoResponse.cs with photo data and metadata

### Infrastructure Implementation

- [X] T073 [US4] Create ImageProcessor in backend/src/AdPhotoManager.Infrastructure/ImageProcessing/ImageProcessor.cs implementing IImageProcessor using SixLabors.ImageSharp
- [X] T074 [US4] Implement circular crop algorithm in ImageProcessor: resize to square, scale to 300×300, apply circular mask
- [X] T075 [US4] Implement quality adjustment logic: reduce quality in 5% increments if output >100KB, minimum 30%
- [X] T076 [US4] Create PhotoService in backend/src/AdPhotoManager.Core/Services/PhotoService.cs implementing IPhotoService
- [X] T077 [US4] Implement AD photo upload in LdapConnectionAdapter: modify thumbnailPhoto attribute via LDAP
- [X] T078 [US4] Implement AD photo retrieval in LdapConnectionAdapter: query thumbnailPhoto attribute
- [X] T079 [US4] Implement AD photo deletion in LdapConnectionAdapter: remove thumbnailPhoto attribute
- [X] T080 [US4] Add photo validation: JPEG format check, max 500KB source size, Turkish error messages

### API Layer

- [X] T081 [US4] Create PhotosController in backend/src/AdPhotoManager.Api/Controllers/PhotosController.cs
- [X] T082 [US4] Add POST /api/users/{id}/photo endpoint with multipart/form-data support
- [X] T083 [US4] Add GET /api/users/{id}/photo endpoint with binary response (image/jpeg) or JSON (base64)
- [X] T084 [US4] Add DELETE /api/users/{id}/photo endpoint
- [ ] T085 [US4] Implement authorization check: users can only upload/delete their own photos unless admin role
- [ ] T086 [US4] Add rate limiting to photo upload endpoint (prevent abuse)

---

## Phase 7: Polish & Cross-Cutting Concerns

**Goal**: Add production-ready features: logging, error handling, health checks, documentation, deployment

**Tasks**:

### Logging & Monitoring

- [X] T087 [P] Configure Serilog in Program.cs with Console, File, and Application Insights sinks
- [X] T088 [P] Add structured logging to all services with correlation IDs
- [X] T089 [P] Create health check endpoints in backend/src/AdPhotoManager.Api/HealthChecks/: DatabaseHealthCheck, AdConnectionHealthCheck
- [X] T090 [P] Register health checks in Program.cs: /health (liveness), /health/ready (readiness)

### Error Handling

- [X] T091 [P] Create GlobalExceptionMiddleware in backend/src/AdPhotoManager.Api/Middleware/GlobalExceptionMiddleware.cs
- [X] T092 [P] Map exceptions to HTTP status codes and Turkish error messages in GlobalExceptionMiddleware
- [ ] T093 [P] Add request/response logging middleware with sanitization (exclude passwords, tokens)

### Validation

- [ ] T094 [P] Install FluentValidation.AspNetCore NuGet package in Api project
- [ ] T095 [P] Create validators for DTOs: LoginRequestValidator, PhotoUploadRequestValidator, UserListRequestValidator in backend/src/AdPhotoManager.Api/Validators/
- [ ] T096 [P] Register FluentValidation in Program.cs

### API Documentation

- [ ] T097 [P] Configure Swagger/OpenAPI in Program.cs with JWT bearer authentication support
- [ ] T098 [P] Add XML documentation comments to controllers and DTOs
- [ ] T099 [P] Enable XML documentation file generation in Api.csproj
- [ ] T100 [P] Add API versioning support (Microsoft.AspNetCore.Mvc.Versioning)

### Security

- [ ] T101 [P] Configure CORS policy in Program.cs to allow frontend origin
- [ ] T102 [P] Add HTTPS redirection middleware in Program.cs
- [ ] T103 [P] Configure security headers middleware: HSTS, X-Content-Type-Options, X-Frame-Options
- [ ] T104 [P] Add rate limiting middleware (AspNetCoreRateLimit) for API endpoints

### Deployment

- [X] T105 [P] Create Dockerfile in backend/docker/Dockerfile for production container
- [X] T106 [P] Create docker-compose.yml in backend/docker/docker-compose.yml for local development (API only, external SQL Server and Hangfire)
- [X] T107 [P] Create deployment documentation in backend/docs/deployment/README.md
- [ ] T108 [P] Create configuration guide in backend/docs/configuration/README.md with all appsettings.json options
- [X] T109 [P] Add environment variable examples in backend/.env.example

### Testing Infrastructure

- [ ] T110 [P] Create test fixtures: TestDbContextFactory, MockLdapConnection in backend/tests/AdPhotoManager.Infrastructure.Tests/Fixtures/
- [ ] T111 [P] Create integration test base class with Testcontainers setup for OpenLDAP in backend/tests/AdPhotoManager.Infrastructure.Tests/IntegrationTestBase.cs
- [ ] T112 [P] Add test data seeders for User and SyncLog entities in backend/tests/Shared/TestDataBuilder.cs

---

## Dependencies & Execution Order

### User Story Dependencies

```
Phase 1 (Setup)
    ↓
Phase 2 (Foundational)
    ↓
    ├─→ Phase 3 (US1: Auth) ──────────┐
    ├─→ Phase 4 (US2: Sync) ──────────┤
    ├─→ Phase 5 (US3: User API) ──────┼─→ Phase 7 (Polish)
    └─→ Phase 6 (US4: Photos) ────────┘
```

**Blocking Dependencies**:
- Phase 2 must complete before any user story phases
- US1 (Auth) should complete before US3 and US4 (they need authentication)
- US2 (Sync) should complete before US3 (user data must exist)
- US4 (Photos) depends on US3 (user management endpoints)

**Parallel Opportunities**:
- Within Phase 2: All [P] tasks can run in parallel (entities, DTOs, constants)
- Within each US phase: Interface definitions and DTOs can be created in parallel
- Phase 7 tasks are mostly independent and can run in parallel

### Suggested Execution Order

**Week 1 - MVP Foundation**:
1. Phase 1: Setup (T001-T017)
2. Phase 2: Foundational (T018-T030)
3. Phase 3: US1 - Authentication (T031-T043)

**Week 2 - Core Features**:
4. Phase 4: US2 - User Sync (T044-T059)
5. Phase 5: US3 - User API (T060-T067)

**Week 3 - Photo Management**:
6. Phase 6: US4 - Photos (T068-T086)

**Week 4 - Production Ready**:
7. Phase 7: Polish (T087-T112)

---

## Parallel Execution Examples

### Phase 2 - Foundational (Parallel)
```bash
# Developer A
T018, T019, T020  # Domain entities and exceptions

# Developer B
T027, T028, T029, T030  # Shared DTOs and constants

# Developer C (waits for A to finish entities)
T021, T022, T023, T024, T025, T026  # Database layer
```

### Phase 3 - US1 Authentication (Parallel)
```bash
# Developer A
T031, T032  # Core interfaces

# Developer B
T033, T034, T035  # DTOs

# Developer C (waits for A)
T036, T037, T038  # Infrastructure implementation

# Developer D (waits for C)
T039, T040, T041, T042, T043  # API layer
```

### Phase 7 - Polish (Highly Parallel)
```bash
# Developer A
T087, T088, T089, T090  # Logging & monitoring

# Developer B
T091, T092, T093  # Error handling

# Developer C
T094, T095, T096  # Validation

# Developer D
T097, T098, T099, T100  # API documentation

# Developer E
T101, T102, T103, T104  # Security

# Developer F
T105, T106, T107, T108, T109  # Deployment

# Developer G
T110, T111, T112  # Testing infrastructure
```

---

## Testing Strategy

**Unit Tests** (70-80% coverage):
- Core/Services: Mock ILdapConnection, IUserRepository
- Infrastructure/ImageProcessing: Test circular crop algorithm with sample images
- Api/Controllers: Mock services, test request/response mapping

**Integration Tests** (20-30% coverage):
- Infrastructure/ActiveDirectory: Use Testcontainers with OpenLDAP
- Infrastructure/Data: Test EF Core queries against real SQL Server (or in-memory)
- Api: Test full request pipeline with TestServer

**Test Execution**:
```bash
# Run all tests
dotnet test backend/

# Run unit tests only
dotnet test backend/ --filter Category=Unit

# Run integration tests only
dotnet test backend/ --filter Category=Integration
```

---

## Success Metrics

**Completion Criteria**:
- ✅ All 112 tasks completed
- ✅ All user stories independently testable
- ✅ Test coverage ≥70% (unit + integration)
- ✅ All API endpoints documented in Swagger
- ✅ Health checks passing
- ✅ Docker container builds successfully
- ✅ Performance targets met (<200ms user listing, <2s photo upload)

**MVP Completion** (Phases 1-4):
- ✅ 59 tasks completed (T001-T059)
- ✅ Users can authenticate via OIDC
- ✅ User data syncs from AD
- ✅ Basic API operational

---

## Notes

- **Existing Infrastructure**: This backend integrates with existing SQL Server and Hangfire projects. No Docker setup needed for database or job scheduler.
- **Turkish Language**: All error messages and user-facing text must be in Turkish.
- **Clean Architecture**: Maintain strict layer separation - Core has no external dependencies.
- **Security First**: All endpoints require authentication except /health and /swagger.
- **Incremental Delivery**: Each user story phase delivers a complete, testable feature increment.
