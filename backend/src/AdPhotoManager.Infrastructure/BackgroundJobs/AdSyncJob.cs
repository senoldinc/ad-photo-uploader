using Microsoft.Extensions.Logging;
using AdPhotoManager.Core.Interfaces;

namespace AdPhotoManager.Infrastructure.BackgroundJobs;

public class AdSyncJob
{
    private readonly IUserSyncService _userSyncService;
    private readonly ILogger<AdSyncJob> _logger;

    public AdSyncJob(
        IUserSyncService userSyncService,
        ILogger<AdSyncJob> logger)
    {
        _userSyncService = userSyncService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("AD sync job started");

        try
        {
            var result = await _userSyncService.SyncUsersAsync(fullSync: false, triggeredBy: "Scheduled");

            _logger.LogInformation(
                "AD sync job completed. Users processed: {Count}, Added: {Added}, Updated: {Updated}, Deleted: {Deleted}",
                result.UsersProcessed, result.UsersAdded, result.UsersUpdated, result.UsersDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AD sync job failed");
            throw;
        }
    }
}
