using HotelChain.Application.Common;
using HotelChain.Application.Images;
using SkiaSharp;

namespace HotelChain.Infrastructure.Storage;

/// <summary>
/// Keeps the visual quality fixed (WebP at <see cref="Quality"/>) and trades resolution for weight:
/// the image is scaled down only as much as needed to fit the byte budget.
/// </summary>
public sealed class SkiaImageOptimizer : IImageOptimizer
{
    /// <summary>High quality where compression artifacts are not visible in a photo.</summary>
    public const int Quality = 85;

    private const int SearchSteps = 4;
    private const double ProbePixels = 500_000;
    private static readonly SKSamplingOptions Sampling = new(SKCubicResampler.Mitchell);

    public Task<OptimizedImage> OptimizeAsync(byte[] image, string extension, long maxBytes, CancellationToken ct = default)
        => Task.Run(() => Optimize(image, extension, maxBytes, ct), ct);

    private static OptimizedImage Optimize(byte[] image, string extension, long maxBytes, CancellationToken ct)
    {
        using var source = Decode(image);

        // Already within budget: keep the original bytes, so there is no quality loss at all.
        if (image.LongLength <= maxBytes)
            return new OptimizedImage(image, extension, source.Width, source.Height);

        // File size grows roughly with the pixel count. A quick probe measures the bytes per pixel
        // of this particular photo, which gives a good estimate of the scale that fits the budget.
        var pixels = (double)source.Width * source.Height;
        double estimate;
        using (var probe = Resize(source, Math.Min(1.0, Math.Sqrt(ProbePixels / pixels))))
        {
            var probeBytes = Encode(probe).Content.Length;
            estimate = Math.Sqrt(maxBytes / ((double)probeBytes / (probe.Width * probe.Height)) / pixels);
        }

        var low = 0.0;
        var high = Math.Min(1.0, estimate * 1.15);
        var best = Encode(source, high);
        if (best.Content.Length <= maxBytes)
        {
            if (high >= 1.0) return best;
            // The estimate was too cautious: look for a larger size, up to the original one.
            (low, high) = (high, 1.0);
        }
        else
        {
            low = Math.Min(1.0, estimate) * 0.85;
            best = Encode(source, low);
            while (best.Content.Length > maxBytes)
            {
                ct.ThrowIfCancellationRequested();
                (high, low) = (low, low * 0.8);
                best = Encode(source, low);
            }
        }

        // Binary search for the largest scale that still fits.
        for (var i = 0; i < SearchSteps; i++)
        {
            ct.ThrowIfCancellationRequested();
            var mid = (low + high) / 2;
            var candidate = Encode(source, mid);
            if (candidate.Content.Length <= maxBytes)
                (best, low) = (candidate, mid);
            else
                high = mid;
        }

        return best;
    }

    /// <summary>Decodes the image applying its EXIF orientation, so phone photos are not stored rotated.</summary>
    private static SKBitmap Decode(byte[] image)
    {
        using var data = SKData.CreateCopy(image);
        using var codec = SKCodec.Create(data);
        var bitmap = codec is null ? null : SKBitmap.Decode(codec);
        if (bitmap is null)
            throw new ValidationException("No se pudo leer la imagen. Usa un archivo JPG, PNG, WEBP o GIF válido.");

        var origin = codec!.EncodedOrigin;
        if (origin == SKEncodedOrigin.TopLeft) return bitmap;

        using (bitmap)
        {
            var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
                or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            var (w, h) = swap ? (bitmap.Height, bitmap.Width) : (bitmap.Width, bitmap.Height);
            var oriented = new SKBitmap(w, h, bitmap.ColorType, bitmap.AlphaType);
            using var canvas = new SKCanvas(oriented);
            canvas.SetMatrix(OriginMatrix(origin, bitmap.Width, bitmap.Height));
            canvas.DrawBitmap(bitmap, 0, 0);
            return oriented;
        }
    }

    private static SKMatrix OriginMatrix(SKEncodedOrigin origin, int w, int h) => origin switch
    {
        SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
        SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
        SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
        SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
        SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
        SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
        SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
        _ => SKMatrix.Identity,
    };

    private static SKBitmap Resize(SKBitmap source, double scale)
    {
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        return source.Resize(new SKImageInfo(width, height, source.ColorType, source.AlphaType), Sampling);
    }

    private static OptimizedImage Encode(SKBitmap source, double scale)
    {
        if (scale >= 1.0) return Encode(source);
        using var resized = Resize(source, scale);
        return Encode(resized);
    }

    private static OptimizedImage Encode(SKBitmap bitmap)
    {
        using var data = bitmap.Encode(SKEncodedImageFormat.Webp, Quality);
        return new OptimizedImage(data.ToArray(), ".webp", bitmap.Width, bitmap.Height);
    }
}
