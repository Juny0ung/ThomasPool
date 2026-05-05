
using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoProfileFormInitializer : IDbInitializer
{
    public async Task EnsureIndexesAsync(IServiceProvider services, IConfiguration configuration)
    {
        var client = services.GetRequiredService<IMongoClient>();
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        var collection = database.GetCollection<ProfileForm>("profileform");

        var model = new CreateIndexModel<ProfileForm>(
            Builders<ProfileForm>.IndexKeys
                .Descending(f => f.Version),
            new CreateIndexOptions { Unique = true }
        );

        await collection.Indexes.CreateOneAsync(model);
    }
}