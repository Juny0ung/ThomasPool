using ThomasPool.Domain.Entities;

namespace ThomasPool.Domain.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(Admin admin);
}