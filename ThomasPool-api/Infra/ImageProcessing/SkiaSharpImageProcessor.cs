using SkiaSharp;
using ThomasPool.Domain.Interfaces;

namespace ThomasPool.Infra.ImageProcessing;

public class SkiaSharpImageProcessor : IImageProcessor
{
    public byte[] Compress(byte[] image, int quality = 75)
    {
        using var bitmap = SKBitmap.Decode(image);
        using var ms = new MemoryStream();
        bitmap.Encode(ms, SKEncodedImageFormat.Jpeg, quality);
        return ms.ToArray();
    }
}
