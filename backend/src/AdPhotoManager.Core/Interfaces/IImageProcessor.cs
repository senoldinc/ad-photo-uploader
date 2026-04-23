namespace AdPhotoManager.Core.Interfaces;

/// <summary>
/// Service for processing images (crop, resize, quality adjustment)
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Create a circular crop of an image at 300x300px
    /// </summary>
    /// <param name="imageData">Source image data</param>
    /// <param name="quality">JPEG quality (0.3 to 1.0)</param>
    /// <returns>Processed image data</returns>
    Task<byte[]> CreateCircularCropAsync(byte[] imageData, double quality = 0.95);

    /// <summary>
    /// Validate photo format and size
    /// </summary>
    /// <param name="imageData">Image data to validate</param>
    /// <param name="maxSizeKb">Maximum size in KB</param>
    /// <returns>Validation result</returns>
    Task<PhotoValidationResult> ValidatePhotoAsync(byte[] imageData, int maxSizeKb = 500);

    /// <summary>
    /// Adjust image quality to meet size constraint
    /// </summary>
    /// <param name="imageData">Image data</param>
    /// <param name="targetSizeKb">Target size in KB</param>
    /// <param name="minQuality">Minimum quality (default 0.3)</param>
    /// <returns>Adjusted image data and quality used</returns>
    Task<(byte[] Data, double Quality)> AdjustQualityAsync(byte[] imageData, int targetSizeKb, double minQuality = 0.3);
}

public class PhotoValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Format { get; set; }
    public int SizeKb { get; set; }
}
