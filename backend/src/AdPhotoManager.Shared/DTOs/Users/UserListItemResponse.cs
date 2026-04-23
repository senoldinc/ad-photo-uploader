namespace AdPhotoManager.Shared.DTOs.Users;

/// <summary>
/// User response model for list views
/// </summary>
public class UserListItemResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public string? Title { get; set; }
    public string? Organization { get; set; }
    public string? Department { get; set; }
    public string? Email { get; set; }
    public bool HasPhoto { get; set; }
    public DateTime? LastPhotoUpdate { get; set; }
    public DateTime LastSyncedAt { get; set; }
}
