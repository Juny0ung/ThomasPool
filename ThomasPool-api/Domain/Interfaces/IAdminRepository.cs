using ThomasPool.Domain.Entities;

namespace ThomasPool.Domain.Interfaces;

public interface IAdminRepository
{
    Task AddUserAsync(string name, string id, string password);
    Task<Admin?> FindUserAsync(string id);
    Task<Admin[]> FindUsersAsync(int skip = 0, int limit = 20, string? name = null, string? id = null, Role? role = null);
    Task ApproveUsersAsync(string[] ids);
}