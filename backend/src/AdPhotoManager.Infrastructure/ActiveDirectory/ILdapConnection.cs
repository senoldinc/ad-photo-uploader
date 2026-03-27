using System.DirectoryServices.Protocols;

namespace AdPhotoManager.Core.Interfaces;

/// <summary>
/// Abstraction over System.DirectoryServices.Protocols for testability
/// </summary>
public interface ILdapConnection
{
    /// <summary>
    /// Executes LDAP search request
    /// </summary>
    SearchResponse Search(SearchRequest request);

    /// <summary>
    /// Executes LDAP modify request
    /// </summary>
    ModifyResponse Modify(ModifyRequest request);

    /// <summary>
    /// Executes LDAP add request
    /// </summary>
    AddResponse Add(AddRequest request);

    /// <summary>
    /// Executes LDAP delete request
    /// </summary>
    DeleteResponse Delete(DeleteRequest request);

    /// <summary>
    /// Tests connection to LDAP server
    /// </summary>
    bool TestConnection();

    /// <summary>
    /// Upload photo to AD user's thumbnailPhoto attribute
    /// </summary>
    Task UploadPhotoAsync(string userDn, byte[] photoData);

    /// <summary>
    /// Get photo from AD user's thumbnailPhoto attribute
    /// </summary>
    Task<byte[]?> GetPhotoAsync(string userDn);

    /// <summary>
    /// Delete photo from AD user's thumbnailPhoto attribute
    /// </summary>
    Task DeletePhotoAsync(string userDn);
}
