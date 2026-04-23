namespace AdPhotoManager.Core.Interfaces;

/// <summary>
/// Service for managing user photos in Active Directory
/// </summary>
public interface IPhotoService
{
    /// <summary>
    /// Upload a photo for a user to Active Directory
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="photoData">Photo data (JPEG format)</param>
    /// <param name="quality">JPEG quality (0.3 to 1.0)</param>
    /// <returns>Upload result with metadata</returns>
    Task<PhotoUploadResult> UploadPhotoAsync(Guid userId, byte[] photoData, double quality = 0.95);

    /// <summary>
    /// Get a user's photo from Active Directory
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Photo data or null if not found</returns>
    Task<byte[]?> GetPhotoAsync(Guid userId);

    /// <summary>
    /// Delete a user's photo from Active Directory
    /// </summary>
    /// <param name="userId">User ID</param>
    Task DeletePhotoAsync(Guid userId);
}

public class PhotoUploadResult
{
    public bool Success { get; set; }
    public int OutputSize { get; set; }
    public double QualityUsed { get; set; }
    public string? ErrorMessage { get; set; }
}
