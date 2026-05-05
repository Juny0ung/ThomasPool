namespace ThomasPool.Domain.Interfaces;

public interface IDbInitializer
{
    Task EnsureIndexesAsync(IServiceProvider services, IConfiguration configuration);
}