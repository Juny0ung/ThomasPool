using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoProfileRepository : IProfileRepository
{
    private readonly IMongoCollection<Profile> _profiles;
    private readonly ILogger<MongoProfileRepository> _logger;


    public MongoProfileRepository(IMongoClient client, IConfiguration configuration, ILogger<MongoProfileRepository> logger)
    {
        _logger = logger;
        
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
            _logger.LogInformation($"Add profile for {profile.Name}");
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException($"Profile already exists: {profile.Name} - {profile.PhoneNumber}");
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
        else _logger.LogInformation($"Updated profile for {profile.Name}");
    }

    public async Task DeleteProfileAsync(ProfileBase profileBase)
    {
        await _profiles.DeleteOneAsync(IdentityFilter(profileBase));
    }

    public async Task<Profile?> GetInfoAsync(ProfileBase profileBase)
    {
        var result = await _profiles.Find(IdentityFilter(profileBase)).FirstOrDefaultAsync();
        if (result == null) _logger.LogWarning($"No profile found for {profileBase.Name}");
        else _logger.LogInformation($"Found profile for {profileBase.Name}");
        return result;
    }

    public async Task<Profile[]> GetInfosAsync(string name, int skip = 0, int limit = 20)
    {
        var profiles = await _profiles.Find(p => p.Name == name)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
        _logger.LogInformation($"Found {profiles.Count()} profiles for {name}");
        return [.. profiles];
    }
}
