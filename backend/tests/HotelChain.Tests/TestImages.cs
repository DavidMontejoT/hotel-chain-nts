using SkiaSharp;

namespace HotelChain.Tests;

internal static class TestImages
{
    /// <summary>A photo-like PNG (gradient plus noise) that weighs well over 400 KB.</summary>
    public static byte[] LargePng(int width = 2400, int height = 1600)
    {
        var random = new Random(42);
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var pixels = new SKColor[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var noise = random.Next(-40, 40);
            pixels[y * width + x] = new SKColor(
                (byte)Math.Clamp(x * 255 / width + noise, 0, 255),
                (byte)Math.Clamp(y * 255 / height + noise, 0, 255),
                (byte)Math.Clamp(160 + noise, 0, 255));
        }
        bitmap.Pixels = pixels;

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>Encodes an image as WebP at the given size, like the optimizer does.</summary>
    public static long WebpSize(byte[] image, int width, int height, int quality)
    {
        using var source = SKBitmap.Decode(image);
        using var resized = source.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKCubicResampler.Mitchell));
        using var data = resized.Encode(SKEncodedImageFormat.Webp, quality);
        return data.Size;
    }
}
