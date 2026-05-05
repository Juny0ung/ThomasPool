
namespace ThomasPool.Domain.Interfaces;

public interface IAdminRepository
{
    Task RegisterAsync(string name, string password);
    Task<bool> LoginAsync(string name, string password);
}