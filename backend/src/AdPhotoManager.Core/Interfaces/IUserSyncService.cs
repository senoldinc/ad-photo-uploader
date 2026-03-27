using AdPhotoManager.Core.Entities;

namespace AdPhotoManager.Core.Interfaces;

public interface IUserSyncService
{
    /// <summary>
    /// Synchronizes users from Active Directory to local database
    /// </summary>
    Task<SyncResult> SyncUsersAsync(bool fullSync = false, string? triggeredBy = null);

    /// <summary>
    /// Gets current sync status
    /// </summary>
    Task<SyncStatusInfo> GetSyncStatusAsync();
}

public class SyncResult
{
    public Guid SyncId { get; set; }
    public SyncStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int UsersProcessed { get; set; }
    public int UsersAdded { get; set; }
    public int UsersUpdated { get; set; }
    public int UsersDeleted { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SyncStatusInfo
{
    public SyncResult? CurrentSync { get; set; }
    public SyncResult? LastSync { get; set; }
}
