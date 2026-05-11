namespace ThomasPool.Domain.Interfaces;

public interface IImageProcessor
{
    byte[] Compress(byte[] image, int quality = 75);
}
