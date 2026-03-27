using Microsoft.EntityFrameworkCore;
using AdPhotoManager.Core.Entities;
using AdPhotoManager.Core.Interfaces;
using AdPhotoManager.Infrastructure.Data;

namespace AdPhotoManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.Users.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(u => !u.IsDeleted);
        }

        return await query.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
    }

    public async Task<User?> GetByAdObjectIdAsync(string adObjectId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AdObjectId == adObjectId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> GetByOrganizationAsync(string organization)
    {
        return await _context.Users
            .Where(u => u.Organization == organization && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.Users.AddAsync(user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        return await Task.FromResult(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
        }
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? organization = null,
        string? department = null)
    {
        var query = _context.Users.Where(u => !u.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                u.DisplayName.ToLower().Contains(searchLower) ||
                (u.EmployeeId != null && u.EmployeeId.ToLower().Contains(searchLower)) ||
                (u.Department != null && u.Department.ToLower().Contains(searchLower)));
        }

        // Apply organization filter
        if (!string.IsNullOrWhiteSpace(organization))
        {
            query = query.Where(u => u.Organization == organization);
        }

        // Apply department filter
        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(u => u.Department == department);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
