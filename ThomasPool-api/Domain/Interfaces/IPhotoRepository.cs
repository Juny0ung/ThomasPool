namespace ThomasPool.Domain.Interfaces;

public interface IPhotoRepository
{
    Task SavePhotoAsync(string photoId, byte[] photo);
    Task<byte[]?> GetPhotoAsync(string photoId);
    Task DeletePhotoAsync(string photoId);
}
