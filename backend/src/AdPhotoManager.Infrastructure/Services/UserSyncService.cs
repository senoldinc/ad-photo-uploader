using System.DirectoryServices.Protocols;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AdPhotoManager.Core.Entities;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Infrastructure.ActiveDirectory;
using AdPhotoManager.Infrastructure.Data;
using AdPhotoManager.Shared.Constants;

namespace AdPhotoManager.Infrastructure.Services;

public class UserSyncService : IUserSyncService
{
    private readonly ILdapConnection _ldapConnection;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserSyncService> _logger;
    private static Guid? _currentSyncId;

    public UserSyncService(
        ILdapConnection ldapConnection,
        IUserRepository userRepository,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<UserSyncService> logger)
    {
        _ldapConnection = ldapConnection;
        _userRepository = userRepository;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SyncResult> SyncUsersAsync(bool fullSync = false, string? triggeredBy = null)
    {
        // Check if sync is already in progress
        if (_currentSyncId.HasValue)
        {
            throw new InvalidOperationException(ErrorCodes.GetMessage(ErrorCodes.SYNC_IN_PROGRESS));
        }

        var syncLog = new SyncLog
        {
            Id = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            Status = SyncStatus.Running,
            TriggerType = string.IsNullOrEmpty(triggeredBy) ? SyncTriggerType.Scheduled : SyncTriggerType.Manual,
            TriggeredBy = triggeredBy
        };

        _currentSyncId = syncLog.Id;

        try
        {
            _context.SyncLogs.Add(syncLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Starting AD sync. SyncId: {SyncId}, FullSync: {FullSync}", syncLog.Id, fullSync);

            // Get users from AD
            var adUsers = await GetUsersFromAdAsync();
            _logger.LogInformation("Retrieved {Count} users from AD", adUsers.Count);

            var existingUsers = (await _userRepository.GetAllAsync(includeDeleted: true)).ToList();

            int added = 0, updated = 0, deleted = 0;

            // Process AD users
            foreach (var adUser in adUsers)
            {
                var existingUser = existingUsers.FirstOrDefault(u => u.AdObjectId == adUser.AdObjectId);

                if (existingUser == null)
                {
                    // Add new user
                    await _userRepository.AddAsync(adUser);
                    added++;
                }
                else
                {
                    // Update existing user
                    existingUser.DisplayName = adUser.DisplayName;
                    existingUser.EmployeeId = adUser.EmployeeId;
                    existingUser.Title = adUser.Title;
                    existingUser.Organization = adUser.Organization;
                    existingUser.Department = adUser.Department;
                    existingUser.Email = adUser.Email;
                    existingUser.HasPhoto = adUser.HasPhoto;
                    existingUser.LastSyncedAt = DateTime.UtcNow;
                    existingUser.IsDeleted = false;

                    await _userRepository.UpdateAsync(existingUser);
                    updated++;
                }
            }

            // Mark users not in AD as deleted
            var adObjectIds = adUsers.Select(u => u.AdObjectId).ToHashSet();
            foreach (var existingUser in existingUsers.Where(u => !u.IsDeleted))
            {
                if (!adObjectIds.Contains(existingUser.AdObjectId))
                {
                    await _userRepository.DeleteAsync(existingUser.Id);
                    deleted++;
                }
            }

            await _userRepository.SaveChangesAsync();

            // Update sync log
            syncLog.CompletedAt = DateTime.UtcNow;
            syncLog.Status = SyncStatus.Completed;
            syncLog.UsersProcessed = adUsers.Count;
            syncLog.UsersAdded = added;
            syncLog.UsersUpdated = updated;
            syncLog.UsersDeleted = deleted;

            _context.SyncLogs.Update(syncLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Sync completed. Added: {Added}, Updated: {Updated}, Deleted: {Deleted}",
                added, updated, deleted);

            return MapToSyncResult(syncLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed");

            syncLog.CompletedAt = DateTime.UtcNow;
            syncLog.Status = SyncStatus.Failed;
            syncLog.ErrorMessage = ex.Message;
            syncLog.ErrorDetails = ex.ToString();

            _context.SyncLogs.Update(syncLog);
            await _context.SaveChangesAsync();

            throw;
        }
        finally
        {
            _currentSyncId = null;
        }
    }

    public async Task<SyncStatusInfo> GetSyncStatusAsync()
    {
        var currentSync = _currentSyncId.HasValue
            ? await _context.SyncLogs.FindAsync(_currentSyncId.Value)
            : null;

        var lastSync = await _context.SyncLogs
            .Where(s => s.Status == SyncStatus.Completed || s.Status == SyncStatus.Failed)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        return new SyncStatusInfo
        {
            CurrentSync = currentSync != null ? MapToSyncResult(currentSync) : null,
            LastSync = lastSync != null ? MapToSyncResult(lastSync) : null
        };
    }

    private async Task<List<User>> GetUsersFromAdAsync()
    {
        var baseDn = _configuration[ConfigurationKeys.AdBaseDn]
            ?? throw new InvalidOperationException("AD Base DN not configured");

        var allowedOrganizations = _configuration
            .GetSection(ConfigurationKeys.AdAllowedOrganizations)
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();

        var users = new List<User>();

        // Build LDAP filter for allowed organizations
        var filter = "(&(objectClass=user)(objectCategory=person))";

        var searchRequest = new SearchRequest(
            baseDn,
            filter,
            SearchScope.Subtree,
            "objectGUID", "displayName", "employeeID", "title", "department", "mail", "thumbnailPhoto"
        );

        var searchResponse = _ldapConnection.Search(searchRequest);

        foreach (SearchResultEntry entry in searchResponse.Entries)
        {
            // Check if user is in allowed organization
            if (allowedOrganizations.Length > 0)
            {
                var isAllowed = allowedOrganizations.Any(org =>
                    entry.DistinguishedName.Contains(org, StringComparison.OrdinalIgnoreCase));

                if (!isAllowed)
                    continue;
            }

            var user = AdUserMapper.MapToUser(entry);
            users.Add(user);
        }

        return await Task.FromResult(users);
    }

    private static SyncResult MapToSyncResult(SyncLog syncLog)
    {
        return new SyncResult
        {
            SyncId = syncLog.Id,
            Status = syncLog.Status,
            StartedAt = syncLog.StartedAt,
            CompletedAt = syncLog.CompletedAt,
            UsersProcessed = syncLog.UsersProcessed,
            UsersAdded = syncLog.UsersAdded,
            UsersUpdated = syncLog.UsersUpdated,
            UsersDeleted = syncLog.UsersDeleted,
            ErrorMessage = syncLog.ErrorMessage
        };
    }
}
