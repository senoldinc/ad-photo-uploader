namespace AdPhotoManager.Shared.DTOs.Photos;

/// <summary>
/// Response model for photo retrieval
/// </summary>
public class PhotoResponse
{
    public string PhotoData { get; set; } = string.Empty; // Base64 encoded
    public int SizeKb { get; set; }
    public DateTime? LastUpdated { get; set; }
}
