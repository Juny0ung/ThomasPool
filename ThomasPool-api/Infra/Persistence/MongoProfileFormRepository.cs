using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoProfileFormRepository : IProfileFormRepository
{
    private readonly IMongoCollection<ProfileForm> _forms;
    private readonly int _maxRetries;
    public MongoProfileFormRepository(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _forms = database.GetCollection<ProfileForm>("profileform");
        
        if (!int.TryParse(configuration["MongoDB:MaxRetries"], out _maxRetries))
        {
            _maxRetries = 3;
            // Add error log
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
        }
        throw new InvalidOperationException($"Failed to insert form after {_maxRetries} retries due to version conflict");
    }

    public async Task<ProfileForm?> GetFormAsync(int? version)
    {
        if (version != null) return await _forms.Find(f => f.Version == version).FirstOrDefaultAsync();
        return await _forms.Find(Builders<ProfileForm>.Filter.Empty)
            .SortByDescending(f => f.Version)
            .Limit(1)
            .FirstOrDefaultAsync();
    }

    private async Task<int> GetLatestVersionAsync()
    {
        var result = await _forms.Find(Builders<ProfileForm>.Filter.Empty)
            .Project(f => new { f.Version }) 
            .SortByDescending(f => f.Version)
            .Limit(1)
            .FirstOrDefaultAsync();

        return result?.Version ?? -1;
    }
}