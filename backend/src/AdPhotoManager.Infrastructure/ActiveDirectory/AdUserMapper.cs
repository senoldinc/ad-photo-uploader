using System.DirectoryServices.Protocols;
using AdPhotoManager.Core.Entities;

namespace AdPhotoManager.Infrastructure.ActiveDirectory;

public static class AdUserMapper
{
    public static User MapToUser(SearchResultEntry entry)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            AdObjectId = GetAttributeValue(entry, "objectGUID") ?? Guid.NewGuid().ToString(),
            DisplayName = GetAttributeValue(entry, "displayName") ?? string.Empty,
            EmployeeId = GetAttributeValue(entry, "employeeID") ?? string.Empty,
            Title = GetAttributeValue(entry, "title") ?? string.Empty,
            Organization = GetOrganizationFromDn(entry.DistinguishedName),
            Department = GetAttributeValue(entry, "department") ?? string.Empty,
            Email = GetAttributeValue(entry, "mail") ?? string.Empty,
            HasPhoto = entry.Attributes.Contains("thumbnailPhoto"),
            LastSyncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static void UpdateUserFromEntry(User user, SearchResultEntry entry)
    {
        user.DisplayName = GetAttributeValue(entry, "displayName") ?? user.DisplayName;
        user.EmployeeId = GetAttributeValue(entry, "employeeID") ?? user.EmployeeId;
        user.Title = GetAttributeValue(entry, "title") ?? user.Title;
        user.Organization = GetOrganizationFromDn(entry.DistinguishedName);
        user.Department = GetAttributeValue(entry, "department") ?? user.Department;
        user.Email = GetAttributeValue(entry, "mail") ?? user.Email;
        user.HasPhoto = entry.Attributes.Contains("thumbnailPhoto");
        user.LastSyncedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.IsDeleted = false;
    }

    private static string? GetAttributeValue(SearchResultEntry entry, string attributeName)
    {
        if (!entry.Attributes.Contains(attributeName))
            return null;

        var attribute = entry.Attributes[attributeName];
        if (attribute.Count == 0)
            return null;

        // Handle GUID specially
        if (attributeName == "objectGUID" && attribute[0] is byte[] guidBytes)
        {
            return new Guid(guidBytes).ToString();
        }

        return attribute[0]?.ToString();
    }

    private static string GetOrganizationFromDn(string distinguishedName)
    {
        // Extract OU from DN
        // Example: CN=User,OU=IT,OU=Departments,DC=example,DC=local -> IT
        var parts = distinguishedName.Split(',');
        var ouPart = parts.FirstOrDefault(p => p.Trim().StartsWith("OU=", StringComparison.OrdinalIgnoreCase));

        if (ouPart != null)
        {
            return ouPart.Substring(3).Trim();
        }

        return "Unknown";
    }
}
