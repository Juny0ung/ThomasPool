using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.Persistence;

class PhotoDocument
{
    [BsonId]
    public required string Id { get; set; }
    public required byte[] Data { get; set; }
}

public class MongoPhotoRepository : IPhotoRepository
{
    private readonly IMongoCollection<PhotoDocument> _photos;

    public MongoPhotoRepository(IMongoClient client, IConfiguration configuration)
    {
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _photos = database.GetCollection<PhotoDocument>("photos");
    }

    public async Task SavePhotoAsync(string photoId, byte[] photo)
    {
        var filter = Builders<PhotoDocument>.Filter.Eq(p => p.Id, photoId);
        var document = new PhotoDocument { Id = photoId, Data = photo };
        await _photos.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<byte[]?> GetPhotoAsync(string photoId)
    {
        var document = await _photos.Find(p => p.Id == photoId).FirstOrDefaultAsync();
        return document?.Data;
    }

    public async Task DeletePhotoAsync(string photoId)
    {
        await _photos.DeleteOneAsync(p => p.Id == photoId);
    }
}
