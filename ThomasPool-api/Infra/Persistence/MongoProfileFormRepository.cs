using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoProfileFormRepository : IProfileFormRepository
{
    private readonly IMongoCollection<ProfileForm> _forms;
    private readonly ILogger<MongoProfileFormRepository> _logger;
    private readonly int _maxRetries;
    public MongoProfileFormRepository(IMongoClient client, IConfiguration configuration, ILogger<MongoProfileFormRepository> logger)
    {
        _logger = logger;
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _forms = database.GetCollection<ProfileForm>("profileform");
        
        if (!int.TryParse(configuration["MongoDB:MaxRetries"], out _maxRetries))
        {
            _maxRetries = 3;
            _logger.LogWarning($"No MaxRetries configuration - set to default (3)");
        }
    }

    public async Task UpdateFormAsync(ProfileForm form)
    {
        for (int i = 0; i < _maxRetries; i++)
        {
            try
            {
                int lastVersion = await GetLatestVersionAsync();
                form.Version = lastVersion + 1;
                await _forms.InsertOneAsync(form);
                return;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                await Task.Delay(Random.Shared.Next(10, 50));
            }
            _logger.LogWarning($"Retry updating profile form {i + 1} times");
        }
        throw new InvalidOperationException($"Failed to insert form after {_maxRetries} retries due to version conflict");
    }

    public async Task<ProfileForm?> GetFormAsync(int? version)
    {
        if (version != null) 
        {
            var result = await _forms.Find(f => f.Version == version).FirstOrDefaultAsync();
            if (result == null) _logger.LogWarning($"No profile form found for version {version}");
            else _logger.LogInformation($"Found profile form for version {version}");
            return result;
        }
        else
        {
            var result = await _forms.Find(Builders<ProfileForm>.Filter.Empty)
                .SortByDescending(f => f.Version)
                .Limit(1)
                .FirstOrDefaultAsync();
            if (result == null) _logger.LogWarning($"No profile form found");
            else _logger.LogInformation($"Found latest profile form - version {result.Version}");
            return result;
        }
    }

    private async Task<int> GetLatestVersionAsync()
    {
        var result = await _forms.Find(Builders<ProfileForm>.Filter.Empty)
            .Project(f => new { f.Version }) 
            .SortByDescending(f => f.Version)
            .Limit(1)
            .FirstOrDefaultAsync();
        _logger.LogInformation($"Latest version: {result?.Version ?? -1}");
        return result?.Version ?? -1;
    }
}