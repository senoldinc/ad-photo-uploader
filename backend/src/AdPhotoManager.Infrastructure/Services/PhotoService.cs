using Microsoft.Extensions.Logging;
using AdPhotoManager.Core.Entities;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Core.Exceptions;

namespace AdPhotoManager.Infrastructure.Services;

public class PhotoService : IPhotoService
{
    private readonly IUserRepository _userRepository;
    private readonly ILdapConnection _ldapConnection;
    private readonly IImageProcessor _imageProcessor;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(
        IUserRepository userRepository,
        ILdapConnection ldapConnection,
        IImageProcessor imageProcessor,
        ILogger<PhotoService> logger)
    {
        _userRepository = userRepository;
        _ldapConnection = ldapConnection;
        _imageProcessor = imageProcessor;
        _logger = logger;
    }

    public async Task<PhotoUploadResult> UploadPhotoAsync(Guid userId, byte[] photoData, double quality = 0.95)
    {
        try
        {
            // Validate photo
            var validation = await _imageProcessor.ValidatePhotoAsync(photoData, maxSizeKb: 500);
            if (!validation.IsValid)
            {
                return new PhotoUploadResult
                {
                    Success = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException($"Kullanıcı bulunamadı: {userId}");
            }

            // Create circular crop
            var croppedData = await _imageProcessor.CreateCircularCropAsync(photoData, quality);

            // Adjust quality if needed to meet 100KB limit
            var finalData = croppedData;
            var finalQuality = quality;

            if (croppedData.Length / 1024 > 100)
            {
                (finalData, finalQuality) = await _imageProcessor.AdjustQualityAsync(
                    croppedData, targetSizeKb: 100, minQuality: 0.3);
            }

            var outputSizeKb = finalData.Length / 1024;

            // Check if still too large
            if (outputSizeKb > 100)
            {
                return new PhotoUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Fotoğraf 100KB limitine sığdırılamadı (mevcut: {outputSizeKb}KB). Lütfen daha küçük bir fotoğraf deneyin."
                };
            }

            // Upload to AD via LDAP
            await UploadToAdAsync(user.AdObjectId, finalData);

            // Update user record
            user.HasPhoto = true;
            user.PhotoUpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Photo uploaded for user {UserId}, size: {SizeKb}KB, quality: {Quality}",
                userId, outputSizeKb, finalQuality);

            return new PhotoUploadResult
            {
                Success = true,
                OutputSize = outputSizeKb,
                QualityUsed = finalQuality
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload photo for user {UserId}", userId);
            throw;
        }
    }

    public async Task<byte[]?> GetPhotoAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException($"Kullanıcı bulunamadı: {userId}");
            }

            if (!user.HasPhoto)
            {
                return null;
            }

            return await GetFromAdAsync(user.AdObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo for user {UserId}", userId);
            throw;
        }
    }

    public async Task DeletePhotoAsync(Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException($"Kullanıcı bulunamadı: {userId}");
            }

            // Delete from AD
            await DeleteFromAdAsync(user.AdObjectId);

            // Update user record
            user.HasPhoto = false;
            user.PhotoUpdatedAt = null;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Photo deleted for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo for user {UserId}", userId);
            throw;
        }
    }

    private async Task UploadToAdAsync(string adObjectId, byte[] photoData)
    {
        // Get user DN from AD using objectGUID
        var userDn = await GetUserDnByObjectIdAsync(adObjectId);
        if (string.IsNullOrEmpty(userDn))
        {
            throw new InvalidOperationException($"User DN not found for AD Object ID: {adObjectId}");
        }

        await _ldapConnection.UploadPhotoAsync(userDn, photoData);
    }

    private async Task<byte[]?> GetFromAdAsync(string adObjectId)
    {
        var userDn = await GetUserDnByObjectIdAsync(adObjectId);
        if (string.IsNullOrEmpty(userDn))
        {
            throw new InvalidOperationException($"User DN not found for AD Object ID: {adObjectId}");
        }

        return await _ldapConnection.GetPhotoAsync(userDn);
    }

    private async Task DeleteFromAdAsync(string adObjectId)
    {
        var userDn = await GetUserDnByObjectIdAsync(adObjectId);
        if (string.IsNullOrEmpty(userDn))
        {
            throw new InvalidOperationException($"User DN not found for AD Object ID: {adObjectId}");
        }

        await _ldapConnection.DeletePhotoAsync(userDn);
    }

    private async Task<string?> GetUserDnByObjectIdAsync(string adObjectId)
    {
        // Search for user by objectGUID
        var filter = $"(objectGUID={ConvertGuidToLdapFormat(adObjectId)})";
        var searchRequest = new System.DirectoryServices.Protocols.SearchRequest(
            "", // Search from root
            filter,
            System.DirectoryServices.Protocols.SearchScope.Subtree,
            "distinguishedName");

        try
        {
            var response = _ldapConnection.Search(searchRequest);
            if (response.Entries.Count > 0)
            {
                var entry = response.Entries[0];
                var dnAttribute = entry.Attributes["distinguishedName"];
                if (dnAttribute != null && dnAttribute.Count > 0)
                {
                    return dnAttribute[0] as string;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user DN for AD Object ID: {AdObjectId}", adObjectId);
        }

        return await Task.FromResult<string?>(null);
    }

    private static string ConvertGuidToLdapFormat(string guidString)
    {
        // Convert GUID string to LDAP octet string format
        var guid = Guid.Parse(guidString);
        var bytes = guid.ToByteArray();
        return string.Join("", bytes.Select(b => $"\\{b:X2}"));
    }
}
