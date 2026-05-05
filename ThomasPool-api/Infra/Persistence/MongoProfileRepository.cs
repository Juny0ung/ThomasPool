using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
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
        Builders<Profile>.Filter.Eq(x => x.Name, p.Name) &
        Builders<Profile>.Filter.Eq(x => x.PhoneNumber, p.PhoneNumber);

    public async Task RegisterAsync(ProfileBase profileBase)
    {
        Validator.ValidateObject(profileBase, new ValidationContext(profileBase), validateAllProperties: true);
        var profile = new Profile(profileBase);
        await _profiles.InsertOneAsync(profile);
    }

    public async Task UnregisterAsync(ProfileBase profileBase)
    {
        await _profiles.DeleteOneAsync(IdentityFilter(profileBase));
    }

    public async Task SaveInfoAsync(ProfileBase profileBase, int version, JsonNode info)
    {
        Validator.ValidateObject(profileBase, new ValidationContext(profileBase), validateAllProperties: true);
        ArgumentOutOfRangeException.ThrowIfNegative(version);
        info["version"] = version;
        var update = Builders<Profile>.Update
            .Set(p => p.Version, version)
            .Set(p => p.Info, info);

        var result = await _profiles.UpdateOneAsync(IdentityFilter(profileBase), update);
        if (result.MatchedCount == 0)
            throw new KeyNotFoundException($"Profile not found: {profileBase.Name}");
    }

    public async Task<JsonNode?> GetInfoAsync(ProfileBase profileBase)
    {
        Profile profile = await _profiles.Find(IdentityFilter(profileBase)).FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Profile not found: {profileBase.Name}");

        return profile.Info;
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
