namespace AdPhotoManager.Shared.DTOs.Users;

/// <summary>
/// Request model for listing users with pagination and filtering
/// </summary>
public class UserListRequest
{
    /// <summary>
    /// Search query (searches displayName, employeeId, department)
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by organization
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Filter by department
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;
}
