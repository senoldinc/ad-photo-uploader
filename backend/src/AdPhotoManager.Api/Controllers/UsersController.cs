using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Shared.Constants;
using AdPhotoManager.Shared.DTOs;
using AdPhotoManager.Shared.DTOs.Sync;
using AdPhotoManager.Shared.DTOs.Users;

namespace AdPhotoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserSyncService _userSyncService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserSyncService userSyncService,
        IUserRepository userRepository,
        ILogger<UsersController> logger)
    {
        _userSyncService = userSyncService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Trigger manual AD synchronization (admin only)
    /// </summary>
    [HttpPost("sync")]
    public async Task<ActionResult<SyncResponse>> TriggerSync([FromBody] SyncRequest request)
    {
        try
        {
            // TODO: Add admin role check
            var userEmail = User.FindFirst("email")?.Value ?? "Unknown";

            var result = await _userSyncService.SyncUsersAsync(
                fullSync: request.FullSync,
                triggeredBy: userEmail);

            return Accepted(new SyncResponse
            {
                SyncId = result.SyncId,
                Status = result.Status.ToString(),
                StartedAt = result.StartedAt,
                Message = "Senkronizasyon başlatıldı"
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("devam ediyor"))
        {
            return Conflict(new ErrorResponse(
                ErrorCodes.SYNC_IN_PROGRESS,
                ErrorCodes.GetMessage(ErrorCodes.SYNC_IN_PROGRESS)
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual sync failed");
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.AD_CONNECTION_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.AD_CONNECTION_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Get current sync status
    /// </summary>
    [HttpGet("sync/status")]
    public async Task<ActionResult<SyncStatusResponse>> GetSyncStatus()
    {
        try
        {
            var status = await _userSyncService.GetSyncStatusAsync();

            return Ok(new SyncStatusResponse
            {
                CurrentSync = status.CurrentSync != null ? new CurrentSyncInfo
                {
                    SyncId = status.CurrentSync.SyncId,
                    Status = status.CurrentSync.Status.ToString(),
                    StartedAt = status.CurrentSync.StartedAt,
                    UsersProcessed = status.CurrentSync.UsersProcessed,
                    TriggerType = "Manual", // TODO: Get from sync result
                    TriggeredBy = null
                } : null,
                LastSync = status.LastSync != null ? new LastSyncInfo
                {
                    SyncId = status.LastSync.SyncId,
                    Status = status.LastSync.Status.ToString(),
                    StartedAt = status.LastSync.StartedAt,
                    CompletedAt = status.LastSync.CompletedAt,
                    UsersProcessed = status.LastSync.UsersProcessed,
                    UsersAdded = status.LastSync.UsersAdded,
                    UsersUpdated = status.LastSync.UsersUpdated,
                    UsersDeleted = status.LastSync.UsersDeleted,
                    TriggerType = "Scheduled"
                } : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sync status");
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.DATABASE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.DATABASE_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Get paginated list of users with optional search and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<UserResponse>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? organization,
        [FromQuery] string? department,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var (users, totalCount) = await _userRepository.GetPagedAsync(
                page, pageSize, search, organization, department);

            var userResponses = users.Select(u => new UserResponse
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                EmployeeId = u.EmployeeId,
                Title = u.Title,
                Organization = u.Organization,
                Department = u.Department,
                Email = u.Email,
                HasPhoto = u.HasPhoto,
                LastPhotoUpdate = u.PhotoUpdatedAt,
                LastSyncedAt = u.LastSyncedAt
            }).ToList();

            return Ok(new PaginationResponse<UserResponse>(
                userResponses,
                page,
                pageSize,
                totalCount
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get users");
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.DATABASE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.DATABASE_ERROR),
                ex.Message
            ));
        }
    }

    /// <summary>
    /// Get user details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailResponse>> GetUserById(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ErrorResponse(
                    ErrorCodes.USER_NOT_FOUND,
                    ErrorCodes.GetMessage(ErrorCodes.USER_NOT_FOUND)
                ));
            }

            var response = new UserDetailResponse
            {
                Id = user.Id,
                AdObjectId = Guid.Parse(user.AdObjectId),
                DisplayName = user.DisplayName,
                EmployeeId = user.EmployeeId,
                Title = user.Title,
                Organization = user.Organization,
                Department = user.Department,
                Email = user.Email,
                PhoneNumber = null, // Not available in current User entity
                Manager = null, // Not available in current User entity
                HasPhoto = user.HasPhoto,
                LastPhotoUpdate = user.PhotoUpdatedAt,
                LastSyncedAt = user.LastSyncedAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", id);
            return StatusCode(500, new ErrorResponse(
                ErrorCodes.DATABASE_ERROR,
                ErrorCodes.GetMessage(ErrorCodes.DATABASE_ERROR),
                ex.Message
            ));
        }
    }
}
