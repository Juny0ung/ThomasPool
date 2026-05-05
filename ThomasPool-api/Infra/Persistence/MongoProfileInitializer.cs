using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoProfileInitializer : IDbInitializer
{
    public async Task EnsureIndexesAsync(IServiceProvider services, IConfiguration configuration)
    {
        var client = services.GetRequiredService<IMongoClient>();
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        var collection = database.GetCollection<Profile>("profiles");

        var uniqueIdentity = new CreateIndexModel<Profile>(
            Builders<Profile>.IndexKeys
                .Ascending(p => p.Name)
                .Ascending(p => p.PhoneNumber),
            new CreateIndexOptions { Unique = true }
        );

        var filterIndex = new CreateIndexModel<Profile>(
            Builders<Profile>.IndexKeys
                .Ascending(p => p.BirthYear)
                .Ascending(p => p.Region)
        );

        await collection.Indexes.CreateManyAsync([uniqueIdentity, filterIndex]);
    }
}
