using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoAdminRepository : IAdminRepository
{
    private readonly IMongoCollection<Admin> _admins;
    private readonly ILogger<MongoAdminRepository> _logger;

    public MongoAdminRepository(IMongoClient client, IConfiguration configuration, ILogger<MongoAdminRepository> logger)
    {
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _admins = database.GetCollection<Admin>("admin");
        _logger = logger;
    }

    public async Task AddUserAsync(string name, string id, string password)
    {
        var admin = new Admin
        {
            Name = name,
            AdminId = id,
            Password = password,
            Role = Role.Pending
        };
        try
        {
            await _admins.InsertOneAsync(admin);
            _logger.LogInformation($"Add user {name}");
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning($"Fail adding user {name}");
            throw new InvalidOperationException($"Id already exists: {id}");
        }
    }

    public async Task<Admin?> FindUserAsync(string id)
    {
        var admin = await _admins.Find(a => a.AdminId == id).FirstOrDefaultAsync();
        if (admin == null) _logger.LogWarning($"No user with id {id}");
        else _logger.LogInformation($"Found user wit id {id} - {admin.Name}");
        return admin;
    }

    public async Task<Admin[]> FindUsersAsync(int skip = 0, int limit = 20, string? name = null, string? id = null, Role? role = null)
    {
        var filter = Builders<Admin>.Filter.Empty;
        if (name != null) filter &= Builders<Admin>.Filter.Eq(a => a.Name, name);
        if (id != null)   filter &= Builders<Admin>.Filter.Eq(a => a.AdminId, id);
        if (role != null) filter &= Builders<Admin>.Filter.Eq(a => a.Role, role);

        var admins = await _admins.Find(filter)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
        _logger.LogInformation($"Found {admins.Count()} users");
        return [.. admins];
    }

    public async Task ApproveUsersAsync(string[] ids)
    {
        var filter = Builders<Admin>.Filter.In(a => a.AdminId, ids);
        var update = Builders<Admin>.Update.Set(a => a.Role, Role.Admin);
        var result = await _admins.UpdateManyAsync(filter, update);
        if (result.MatchedCount != ids.Length)
            throw new KeyNotFoundException($"Some ids not found ({result.MatchedCount}/{ids.Length} matched)");
        else _logger.LogInformation($"Approved {result.MatchedCount} users");
    }
}