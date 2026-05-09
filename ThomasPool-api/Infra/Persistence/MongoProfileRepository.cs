using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoProfileRepository : IProfileRepository
{
    private readonly IMongoCollection<Profile> _profiles;

    public MongoProfileRepository(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _profiles = database.GetCollection<Profile>("profiles");
    }

    private static FilterDefinition<Profile> IdentityFilter(ProfileBase p) =>
        Builders<Profile>.Filter.Eq(x => x.PhoneNumber, p.PhoneNumber);

    public async Task SaveInfoAsync(Profile profile)
    {
        try
        {
            await _profiles.InsertOneAsync(profile);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException($"Profile already exists: {profile.PhoneNumber}");
        }
    }

    public async Task UpdateInfoAsync(Profile profile)
    {
        var update = Builders<Profile>.Update
            .Set(p => p.Version, profile.Version)
            .Set(p => p.Info, profile.Info);

        var result = await _profiles.UpdateOneAsync(IdentityFilter(profile), update);
        if (result.MatchedCount == 0)
            throw new KeyNotFoundException($"Profile not found: {profile.Name}");
    }

    public async Task DeleteProfileAsync(ProfileBase profileBase)
    {
        await _profiles.DeleteOneAsync(IdentityFilter(profileBase));
    }

    public async Task<Profile?> GetInfoAsync(ProfileBase profileBase)
    {
        return await _profiles.Find(IdentityFilter(profileBase)).FirstOrDefaultAsync();
    }

    public async Task<Profile[]> GetInfosAsync(string name, int skip = 0, int limit = 20)
    {
        var profiles = await _profiles.Find(p => p.Name == name)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
        return [.. profiles];
    }
}
