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
