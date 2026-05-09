using MongoDB.Driver;
using ThomasPool.Domain.Entities;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

public class MongoAdminRepository : IAdminRepository
{
    private readonly IMongoCollection<Admin> _admins;
    public MongoAdminRepository(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _admins = database.GetCollection<Admin>("admin");
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
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException($"Id already exists: {id}");
        }
    }

    public async Task<Admin?> FindUserAsync(string id)
    {
        return await _admins.Find(a => a.AdminId == id).FirstOrDefaultAsync();
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
        return [.. admins];
    }

    public async Task ApproveUsersAsync(string[] ids)
    {
        var filter = Builders<Admin>.Filter.In(a => a.AdminId, ids);
        var update = Builders<Admin>.Update.Set(a => a.Role, Role.Admin);
        var result = await _admins.UpdateManyAsync(filter, update);
        if (result.MatchedCount != ids.Length)
            throw new KeyNotFoundException($"Some ids not found ({result.MatchedCount}/{ids.Length} matched)");
    }
}