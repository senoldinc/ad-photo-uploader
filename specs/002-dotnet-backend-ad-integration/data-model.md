# Data Model: .NET 8 Backend with Active Directory Integration

**Date**: 2026-03-26
**Feature**: 002-dotnet-backend-ad-integration

## Overview

This document defines the domain entities, database schema, and data relationships for the AD Photo Manager backend.

---

## 1. Domain Entities

### User Entity

**Purpose**: Local cache of Active Directory user data for performance and offline capability.

```csharp
namespace AdPhotoManager.Core.Entities;

public class User
{
    /// <summary>
    /// Primary key (auto-generated GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Active Directory ObjectGUID (unique identifier from AD)
    /// </summary>
    public string AdObjectId { get; set; } = string.Empty;

    /// <summary>
    /// User's display name from AD (e.g., "Ahmet Yılmaz")
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Employee ID from AD (e.g., "SİC-00001")
    /// </summary>
    public string EmployeeId { get; set; } = string.Empty;

    /// <summary>
    /// Job title from AD (e.g., "Yazılım Geliştirici")
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Organization name from AD (e.g., "Bilgi Teknolojileri")
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// Department name from AD (e.g., "Yazılım Geliştirme")
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Email address from AD
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if user has a photo in AD
    /// </summary>
    public bool HasPhoto { get; set; }

    /// <summary>
    /// Timestamp of last successful sync from AD
    /// </summary>
    public DateTime LastSyncedAt { get; set; }

    /// <summary>
    /// Timestamp when photo was last updated
    /// </summary>
    public DateTime? PhotoUpdatedAt { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Record last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag (for audit trail)
    /// </summary>
    public bool IsDeleted { get; set; }
}
```

### SyncLog Entity

**Purpose**: Track AD synchronization history for monitoring and troubleshooting.

```csharp
namespace AdPhotoManager.Core.Entities;

public class SyncLog
{
    public Guid Id { get; set; }

    /// <summary>
    /// Sync start timestamp
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Sync completion timestamp
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Sync status: Pending, Running, Completed, Failed
    /// </summary>
    public SyncStatus Status { get; set; }

    /// <summary>
    /// Number of users processed
    /// </summary>
    public int UsersProcessed { get; set; }

    /// <summary>
    /// Number of users added
    /// </summary>
    public int UsersAdded { get; set; }

    /// <summary>
    /// Number of users updated
    /// </summary>
    public int UsersUpdated { get; set; }

    /// <summary>
    /// Number of users marked as deleted
    /// </summary>
    public int UsersDeleted { get; set; }

    /// <summary>
    /// Error message if sync failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detailed error stack trace
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Sync trigger type: Scheduled, Manual
    /// </summary>
    public SyncTriggerType TriggerType { get; set; }

    /// <summary>
    /// User who triggered manual sync (if applicable)
    /// </summary>
    public string? TriggeredBy { get; set; }
}

public enum SyncStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

public enum SyncTriggerType
{
    Scheduled = 0,
    Manual = 1
}
```

---

## 2. Database Schema

### Users Table

```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AdObjectId NVARCHAR(255) NOT NULL UNIQUE,
    DisplayName NVARCHAR(255) NOT NULL,
    EmployeeId NVARCHAR(50) NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Organization NVARCHAR(255) NOT NULL,
    Department NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    HasPhoto BIT NOT NULL DEFAULT 0,
    LastSyncedAt DATETIME2 NOT NULL,
    PhotoUpdatedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Indexes for common queries
CREATE INDEX idx_users_employeeid ON Users(EmployeeId);
CREATE INDEX idx_users_department ON Users(Department);
CREATE INDEX idx_users_organization ON Users(Organization);
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_displayname ON Users(DisplayName);
CREATE NONCLUSTERED INDEX idx_users_isdeleted ON Users(IsDeleted) WHERE IsDeleted = 0;
```

### SyncLogs Table

```sql
CREATE TABLE SyncLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    StartedAt DATETIME2 NOT NULL,
    CompletedAt DATETIME2 NULL,
    Status INT NOT NULL,
    UsersProcessed INT NOT NULL DEFAULT 0,
    UsersAdded INT NOT NULL DEFAULT 0,
    UsersUpdated INT NOT NULL DEFAULT 0,
    UsersDeleted INT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(MAX) NULL,
    ErrorDetails NVARCHAR(MAX) NULL,
    TriggerType INT NOT NULL,
    TriggeredBy NVARCHAR(255) NULL
);

-- Index for monitoring recent syncs
CREATE INDEX idx_synclogs_startedat ON SyncLogs(StartedAt DESC);
CREATE INDEX idx_synclogs_status ON SyncLogs(Status);
```

---

## 3. Entity Framework Core Configuration

### UserConfiguration

```csharp
namespace AdPhotoManager.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.AdObjectId)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.AdObjectId)
            .IsUnique();

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.EmployeeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.EmployeeId);

        builder.Property(u => u.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Organization)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Organization);

        builder.Property(u => u.Department)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Department);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email);

        builder.Property(u => u.HasPhoto)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.LastSyncedAt)
            .IsRequired();

        builder.Property(u => u.PhotoUpdatedAt)
            .IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(u => u.IsDeleted)
            .HasFilter("IsDeleted = false");
    }
}
```

---

## 4. Validation Rules

### User Entity Validation

```csharp
namespace AdPhotoManager.Core.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.AdObjectId)
            .NotEmpty()
            .WithMessage("AD Object ID gereklidir");

        RuleFor(u => u.DisplayName)
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Görünen ad gereklidir ve 255 karakterden kısa olmalıdır");

        RuleFor(u => u.EmployeeId)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Çalışan numarası gereklidir");

        RuleFor(u => u.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi gereklidir");

        RuleFor(u => u.Organization)
            .NotEmpty()
            .WithMessage("Organizasyon gereklidir");

        RuleFor(u => u.Department)
            .NotEmpty()
            .WithMessage("Departman gereklidir");
    }
}
```

### Photo Upload Validation

```csharp
public class PhotoUploadValidator
{
    public const int MaxSourceSizeBytes = 500 * 1024; // 500 KB
    public const int MaxOutputSizeBytes = 100 * 1024; // 100 KB
    public const int OutputDimension = 300; // 300x300 px

    public static ValidationResult ValidatePhoto(byte[] photoData, string contentType)
    {
        if (photoData == null || photoData.Length == 0)
            return ValidationResult.Failure("Fotoğraf verisi boş olamaz");

        if (photoData.Length > MaxSourceSizeBytes)
            return ValidationResult.Failure($"Fotoğraf boyutu {MaxSourceSizeBytes / 1024} KB'dan küçük olmalıdır");

        if (contentType != "image/jpeg" && contentType != "image/jpg")
            return ValidationResult.Failure("Sadece JPEG formatı desteklenmektedir");

        return ValidationResult.Success();
    }
}
```

---

## 5. State Transitions

### User Sync State Machine

```
[New User in AD]
    ↓
[Created in DB] → LastSyncedAt = Now, IsDeleted = false
    ↓
[Updated in AD] → UpdatedAt = Now, LastSyncedAt = Now
    ↓
[Removed from AD] → IsDeleted = true, UpdatedAt = Now
```

### Photo Upload State Machine

```
[No Photo] → HasPhoto = false, PhotoUpdatedAt = null
    ↓
[Upload Photo] → HasPhoto = true, PhotoUpdatedAt = Now
    ↓
[Update Photo] → PhotoUpdatedAt = Now
    ↓
[Delete Photo] → HasPhoto = false, PhotoUpdatedAt = Now
```

---

## 6. Relationships

**Current scope**: Single entity model (User) with audit logging (SyncLog).

**Future considerations**:
- User roles/permissions (if RBAC expands beyond AD groups)
- Photo upload history/audit trail
- User activity logs

---

## Summary

- **Primary entity**: User (cached AD data)
- **Audit entity**: SyncLog (sync history tracking)
- **Database**: PostgreSQL with optimized indexes for search/filter operations
- **Validation**: Turkish error messages, strict photo size/format rules
- **State management**: Soft deletes for audit trail, timestamp tracking for sync status
