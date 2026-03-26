namespace AdPhotoManager.Core.Entities;

public class SyncLog
{
    public Guid Id { get; set; }

    /// <summary>
    /// Sync start timestamp
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Sync completion timestamp
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Sync status: Pending, Running, Completed, Failed
    /// </summary>
    public SyncStatus Status { get; set; }

    /// <summary>
    /// Number of users processed
    /// </summary>
    public int UsersProcessed { get; set; }

    /// <summary>
    /// Number of users added
    /// </summary>
    public int UsersAdded { get; set; }

    /// <summary>
    /// Number of users updated
    /// </summary>
    public int UsersUpdated { get; set; }

    /// <summary>
    /// Number of users marked as deleted
    /// </summary>
    public int UsersDeleted { get; set; }

    /// <summary>
    /// Error message if sync failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Detailed error stack trace
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Sync trigger type: Scheduled, Manual
    /// </summary>
    public SyncTriggerType TriggerType { get; set; }

    /// <summary>
    /// User who triggered manual sync (if applicable)
    /// </summary>
    public string? TriggeredBy { get; set; }
}

public enum SyncStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

public enum SyncTriggerType
{
    Scheduled = 0,
    Manual = 1
}
