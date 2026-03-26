namespace AdPhotoManager.Shared.DTOs.Sync;

public class SyncResponse
{
    public Guid SyncId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}
