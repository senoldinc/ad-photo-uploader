namespace AdPhotoManager.Shared.DTOs.Sync;

public class SyncStatusResponse
{
    public CurrentSyncInfo? CurrentSync { get; set; }
    public LastSyncInfo? LastSync { get; set; }
}

public class CurrentSyncInfo
{
    public Guid SyncId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public int UsersProcessed { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public string? TriggeredBy { get; set; }
}

public class LastSyncInfo
{
    public Guid SyncId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int UsersProcessed { get; set; }
    public int UsersAdded { get; set; }
    public int UsersUpdated { get; set; }
    public int UsersDeleted { get; set; }
    public string TriggerType { get; set; } = string.Empty;
}
