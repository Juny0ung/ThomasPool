
using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoAdminInitializer : IDbInitializer
{
    public async Task EnsureIndexesAsync(IServiceProvider services, IConfiguration configuration)
    {
        var client = services.GetRequiredService<IMongoClient>();
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        var collection = database.GetCollection<Admin>("admin");

        var model = new CreateIndexModel<Admin>(
            Builders<Admin>.IndexKeys
                .Ascending(a => a.Id),
            new CreateIndexOptions { Unique = true }
        );

        await collection.Indexes.CreateOneAsync(model);
    }
}