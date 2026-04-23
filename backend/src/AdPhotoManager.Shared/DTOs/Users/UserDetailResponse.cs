namespace AdPhotoManager.Shared.DTOs.Users;

/// <summary>
/// Detailed user response model
/// </summary>
public class UserDetailResponse
{
    public Guid Id { get; set; }
    public Guid AdObjectId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? Title { get; set; }
    public string? Organization { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Manager { get; set; }
    public bool HasPhoto { get; set; }
    public DateTime? LastPhotoUpdate { get; set; }
    public DateTime LastSyncedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
