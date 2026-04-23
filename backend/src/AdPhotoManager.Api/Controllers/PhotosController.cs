using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Shared.Constants;
using AdPhotoManager.Shared.DTOs;
using AdPhotoManager.Shared.DTOs.Photos;

namespace AdPhotoManager.Api.Controllers;

[ApiController]
[Route("api/users/{userId}/photo")]
[Authorize]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PhotosController> _logger;

    public PhotosController(
        IPhotoService photoService,
        IUserRepository userRepository,
        ILogger<PhotosController> logger)
    {
        _photoService = photoService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Upload a photo for a user
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(524288)] // 512KB max request size
    public async Task<ActionResult<PhotoUploadResponse>> UploadPhoto(
        Guid userId,
        [FromForm] IFormFile photo,
        [FromForm] double quality = 0.95)
    {
        try
        {
            // Validate file
            if (photo == null || photo.Length == 0)
            {
                return BadRequest(new ErrorResponse(
                    ErrorCodes.PHOTO_VALIDATION_ERROR,
                    "Fotoğraf dosyası gereklidir"
                ));
            }

            // Check file size (500KB max)
            if (photo.Length > 500 * 1024)
            {
                return BadRequest(new ErrorResponse(
                    ErrorCodes.PHOTO_VALIDATION_ERROR,
                    "Fotoğraf boyutu 500KB'dan büyük olamaz"
                ));
            }

            // Check content type
            if (!photo.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) &&
                !photo.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ErrorResponse(
                    ErrorCodes.PHOTO_VALIDATION_ERROR,
                    "Sadece JPEG formatı desteklenmektedir"
                ));
            }

            // Validate quality
            if (quality < 0.3 || quality > 1.0)
            {
                return BadRequest(new ErrorResponse(
                    ErrorCodes.PHOTO_VALIDATION_ERROR,
                    "Kalite değeri 0.3 ile 1.0 arasında olmalıdır"
                ));
            }

            // Read photo data
            byte[] photoData;
            using (var ms = new MemoryStream())
            {
                await photo.CopyToAsync(ms);
                photoData = ms.ToArray();
            }

            // Upload photo
            var result = await _photoService.UploadPhotoAsync(userId, photoData, quality);

            if (!result.Success)
            {
                return BadRequest(new ErrorResponse(
                    ErrorCodes.PHOTO_VALIDATION_ERROR,
                    result.ErrorMessage ?? "Fotoğraf yüklenemedi"
                ));
            }

            return Ok(new PhotoUploadResponse
            {
                Success = true,
                Message = "Fotoğraf başarıyla yüklendi",
                OutputSizeKb = result.OutputSize,
                QualityUsed = result.QualityUsed,
                UploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload photo for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.PHOTO_UPLOAD_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.PHOTO_UPLOAD_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Get a user's photo
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPhoto(Guid userId, [FromQuery] bool asJson = false)
    {
        try
        {
            var photoData = await _photoService.GetPhotoAsync(userId);

            if (photoData == null)
            {
                return NotFound(new ErrorResponse(
                    ErrorCodes.PHOTO_NOT_FOUND,
                    "Kullanıcının fotoğrafı bulunamadı"
                ));
            }

            if (asJson)
            {
                // Return as JSON with base64 encoded data
                var user = await _userRepository.GetByIdAsync(userId);
                return Ok(new PhotoResponse
                {
                    PhotoData = Convert.ToBase64String(photoData),
                    SizeKb = photoData.Length / 1024,
                    LastUpdated = user?.PhotoUpdatedAt
                });
            }

            // Return as binary image
            return File(photoData, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.DATABASE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.DATABASE_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Delete a user's photo
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeletePhoto(Guid userId)
    {
        try
        {
            await _photoService.DeletePhotoAsync(userId);

            return Ok(new
            {
                success = true,
                message = "Fotoğraf başarıyla silindi"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo for user {UserId}", userId);
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.PHOTO_DELETE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.PHOTO_DELETE_ERROR),
                ex.Message
            ));
        }
    }
}
