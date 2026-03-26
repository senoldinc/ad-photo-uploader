using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using AdPhotoManager.Core.Interfaces;

namespace AdPhotoManager.Infrastructure.ImageProcessing;

public class ImageProcessor : IImageProcessor
{
    private const int TargetSize = 300;

    public async Task<byte[]> CreateCircularCropAsync(byte[] imageData, double quality = 0.95)
    {
        using var inputStream = new MemoryStream(imageData);
        using var image = await Image.LoadAsync(inputStream);

        // Resize to 300x300 square
        // Note: Frontend already handles circular cropping with canvas
        // Backend just ensures 300x300 size and quality optimization
        image.Mutate(x => x.Resize(TargetSize, TargetSize));

        // Convert to JPEG with specified quality
        using var ms = new MemoryStream();
        var encoder = new JpegEncoder
        {
            Quality = (int)(quality * 100)
        };
        await image.SaveAsync(ms, encoder);

        return ms.ToArray();
    }

    public async Task<PhotoValidationResult> ValidatePhotoAsync(byte[] imageData, int maxSizeKb = 500)
    {
        var result = new PhotoValidationResult();

        // Check size
        var sizeKb = imageData.Length / 1024;
        result.SizeKb = sizeKb;

        if (sizeKb > maxSizeKb)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Fotoğraf boyutu {maxSizeKb}KB'dan büyük olamaz (mevcut: {sizeKb}KB)";
            return result;
        }

        try
        {
            using var inputStream = new MemoryStream(imageData);
            using var image = await Image.LoadAsync(inputStream);
            result.Format = image.Metadata.DecodedImageFormat?.Name ?? "Unknown";

            // Check if JPEG
            if (!result.Format.Equals("JPEG", StringComparison.OrdinalIgnoreCase))
            {
                result.IsValid = false;
                result.ErrorMessage = "Sadece JPEG formatı desteklenmektedir";
                return result;
            }

            result.IsValid = true;
            return result;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ErrorMessage = $"Geçersiz resim formatı: {ex.Message}";
            return result;
        }
    }

    public async Task<(byte[] Data, double Quality)> AdjustQualityAsync(
        byte[] imageData,
        int targetSizeKb,
        double minQuality = 0.3)
    {
        var currentQuality = 0.95;
        var currentData = imageData;

        while (currentData.Length / 1024 > targetSizeKb && currentQuality >= minQuality)
        {
            currentQuality -= 0.05;

            using var inputStream = new MemoryStream(imageData);
            using var image = await Image.LoadAsync(inputStream);
            using var ms = new MemoryStream();

            var encoder = new JpegEncoder
            {
                Quality = (int)(currentQuality * 100)
            };

            await image.SaveAsync(ms, encoder);
            currentData = ms.ToArray();
        }

        return (currentData, currentQuality);
    }
}
