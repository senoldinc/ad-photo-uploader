using System.DirectoryServices.Protocols;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Shared.Constants;

namespace AdPhotoManager.Infrastructure.ActiveDirectory;

public class LdapConnectionAdapter : ILdapConnection, IDisposable
{
    private readonly LdapConnection _connection;
    private readonly ILogger<LdapConnectionAdapter> _logger;
    private bool _disposed;

    public LdapConnectionAdapter(
        IConfiguration configuration,
        ILogger<LdapConnectionAdapter> logger)
    {
        _logger = logger;

        var server = configuration[ConfigurationKeys.AdServer]
            ?? throw new InvalidOperationException("AD Server not configured");
        var port = int.Parse(configuration[ConfigurationKeys.AdPort] ?? "389");

        var identifier = new LdapDirectoryIdentifier(server, port);
        _connection = new LdapConnection(identifier);

        var useSsl = bool.Parse(configuration[ConfigurationKeys.AdUseSsl] ?? "false");
        if (useSsl)
        {
            _connection.SessionOptions.SecureSocketLayer = true;
        }

        // Bind with service account
        var serviceAccount = configuration[ConfigurationKeys.AdServiceAccount];
        var servicePassword = configuration[ConfigurationKeys.AdServicePassword];

        if (!string.IsNullOrEmpty(serviceAccount) && !string.IsNullOrEmpty(servicePassword))
        {
            var credential = new System.Net.NetworkCredential(serviceAccount, servicePassword);
            _connection.Bind(credential);
            _logger.LogInformation("LDAP connection established with service account");
        }
        else
        {
            _logger.LogWarning("LDAP connection created without credentials");
        }
    }

    public SearchResponse Search(SearchRequest request)
    {
        try
        {
            var response = (SearchResponse)_connection.SendRequest(request);
            _logger.LogDebug("LDAP search completed: {ResultCount} results", response.Entries.Count);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP search failed");
            throw;
        }
    }

    public ModifyResponse Modify(ModifyRequest request)
    {
        try
        {
            var response = (ModifyResponse)_connection.SendRequest(request);
            _logger.LogDebug("LDAP modify completed");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP modify failed");
            throw;
        }
    }

    public AddResponse Add(AddRequest request)
    {
        try
        {
            var response = (AddResponse)_connection.SendRequest(request);
            _logger.LogDebug("LDAP add completed");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP add failed");
            throw;
        }
    }

    public DeleteResponse Delete(DeleteRequest request)
    {
        try
        {
            var response = (DeleteResponse)_connection.SendRequest(request);
            _logger.LogDebug("LDAP delete completed");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP delete failed");
            throw;
        }
    }

    public bool TestConnection()
    {
        try
        {
            // Simple bind test
            _connection.Bind();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LDAP connection test failed");
            return false;
        }
    }

    public async Task UploadPhotoAsync(string userDn, byte[] photoData)
    {
        try
        {
            var modifyRequest = new ModifyRequest(
                userDn,
                DirectoryAttributeOperation.Replace,
                "thumbnailPhoto",
                photoData);

            var response = (ModifyResponse)_connection.SendRequest(modifyRequest);

            if (response.ResultCode != ResultCode.Success)
            {
                throw new InvalidOperationException(
                    $"LDAP photo upload failed: {response.ResultCode} - {response.ErrorMessage}");
            }

            _logger.LogInformation("Photo uploaded to AD for user DN: {UserDn}", userDn);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload photo to AD for user DN: {UserDn}", userDn);
            throw;
        }
    }

    public async Task<byte[]?> GetPhotoAsync(string userDn)
    {
        try
        {
            var searchRequest = new SearchRequest(
                userDn,
                "(objectClass=*)",
                SearchScope.Base,
                "thumbnailPhoto");

            var response = (SearchResponse)_connection.SendRequest(searchRequest);

            if (response.Entries.Count == 0)
            {
                _logger.LogWarning("User not found in AD: {UserDn}", userDn);
                return null;
            }

            var entry = response.Entries[0];
            var photoAttribute = entry.Attributes["thumbnailPhoto"];

            if (photoAttribute == null || photoAttribute.Count == 0)
            {
                _logger.LogDebug("No photo found for user DN: {UserDn}", userDn);
                return null;
            }

            var photoData = photoAttribute[0] as byte[];
            _logger.LogInformation("Photo retrieved from AD for user DN: {UserDn}, size: {Size} bytes",
                userDn, photoData?.Length ?? 0);

            return await Task.FromResult(photoData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get photo from AD for user DN: {UserDn}", userDn);
            throw;
        }
    }

    public async Task DeletePhotoAsync(string userDn)
    {
        try
        {
            var modifyRequest = new ModifyRequest(
                userDn,
                DirectoryAttributeOperation.Delete,
                "thumbnailPhoto");

            var response = (ModifyResponse)_connection.SendRequest(modifyRequest);

            if (response.ResultCode != ResultCode.Success &&
                response.ResultCode != ResultCode.NoSuchAttribute)
            {
                throw new InvalidOperationException(
                    $"LDAP photo deletion failed: {response.ResultCode} - {response.ErrorMessage}");
            }

            _logger.LogInformation("Photo deleted from AD for user DN: {UserDn}", userDn);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete photo from AD for user DN: {UserDn}", userDn);
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
