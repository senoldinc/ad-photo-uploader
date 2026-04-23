using AdPhotoManager.Core.Entities;

namespace AdPhotoManager.Core.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync(bool includeDeleted = false);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByAdObjectIdAsync(string adObjectId);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByOrganizationAsync(string organization);
    Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? organization = null,
        string? department = null);
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();
}
