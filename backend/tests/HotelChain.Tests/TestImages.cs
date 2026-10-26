using SkiaSharp;

namespace HotelChain.Tests;

internal static class TestImages
{
    /// <summary>A photo-like PNG (gradient plus noise) that weighs well over 400 KB.</summary>
    public static byte[] LargePng(int width = 2400, int height = 1600, int noise = 40)
    {
        var random = new Random(42);
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
        var pixels = new SKColor[width * height];
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var n = random.Next(-noise, noise + 1);
            pixels[y * width + x] = new SKColor(
                (byte)Math.Clamp(x * 255 / width + n, 0, 255),
                (byte)Math.Clamp(y * 255 / height + n, 0, 255),
                (byte)Math.Clamp(160 + n, 0, 255));
        }
        bitmap.Pixels = pixels;

        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    /// <summary>A large image with only soft, low-frequency detail (like a wallpaper): it compresses far better at full size.</summary>
    public static byte[] SmoothPng(int size = 4000)
    {
        var random = new Random(7);
        using var seed = new SKBitmap(24, 24, SKColorType.Rgba8888, SKAlphaType.Opaque);
        seed.Pixels = Enumerable.Range(0, 24 * 24)
            .Select(_ => new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)))
            .ToArray();
        using var smooth = seed.Resize(new SKImageInfo(size, size), new SKSamplingOptions(SKCubicResampler.Mitchell));
        using var data = smooth.Encode(SKEncodedImageFormat.Png, 100);
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
