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
        _admins = database.GetCollection<Admin>("admins");
    }

    public async Task RegisterAsync(string name, string password)
    {
        var admin = new Admin
        {
            Name = name,
            Password = password
        };
        await _admins.InsertOneAsync(admin);
    }

    public async Task<bool> LoginAsync(string name, string password)
    {
        Admin admin = await _admins.Find(a => a.Name == name).FirstOrDefaultAsync();
        return admin?.Password == password;
    }
}