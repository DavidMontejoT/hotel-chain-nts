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

    private const int MaxAttempts = 8;
    private const double Tolerance = 0.02;
    private const double NearlyFull = 0.97;
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

        // File size grows roughly with the pixel count, so each encoding tells how far the target is.
        // A small probe gives the first guess; each attempt then interpolates (in log-log scale) between
        // the two latest measurements, which usually lands within a couple of percent in 2-3 attempts.
        var pixels = (double)source.Width * source.Height;
        var target = maxBytes * 0.99;
        (double Scale, long Bytes) previous;
        using (var probe = Resize(source, Math.Min(1.0, Math.Sqrt(ProbePixels / pixels))))
            previous = ((double)probe.Width / source.Width, Encode(probe).Content.Length);

        OptimizedImage? best = null;
        double low = 0, high = double.PositiveInfinity;
        var scale = Math.Min(1.0, previous.Scale * Math.Sqrt(target / previous.Bytes));

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            var candidate = Encode(source, scale);
            if (candidate.Content.Length <= maxBytes)
            {
                (best, low) = (candidate, scale);
                // Done when it is the original size or it already uses almost the whole budget.
                if (scale >= 1.0 || candidate.Content.Length >= maxBytes * NearlyFull) break;
            }
            else
            {
                high = scale;
            }
            if (!double.IsPositiveInfinity(high) && high - low <= high * Tolerance) break;

            var current = (Scale: scale, Bytes: (long)candidate.Content.Length);
            scale = NextScale(previous, current, target, low, high);
            previous = current;
        }

        // Safety net: keep shrinking until it fits (only reached with very unusual images).
        while (best is null)
        {
            ct.ThrowIfCancellationRequested();
            low = (high == double.PositiveInfinity ? 1.0 : high) * 0.8;
            var candidate = Encode(source, low);
            if (candidate.Content.Length <= maxBytes) best = candidate;
            else high = low;
        }

        return best;
    }

    /// <summary>Predicts the scale that reaches <paramref name="target"/> bytes, kept between the known bounds.</summary>
    private static double NextScale((double Scale, long Bytes) a, (double Scale, long Bytes) b, double target, double low, double high)
    {
        var exponent = Math.Abs(b.Scale - a.Scale) > 1e-6
            ? Math.Log((double)b.Bytes / a.Bytes) / Math.Log(b.Scale / a.Scale)
            : 2.0;
        if (!double.IsFinite(exponent) || exponent < 0.5 || exponent > 4) exponent = 2.0;

        var next = Math.Min(1.0, b.Scale * Math.Pow(target / b.Bytes, 1 / exponent));
        // Never repeat a known result: stay strictly inside the bracket of what fits and what does not.
        if (next >= high) next = low > 0 ? (low + high) / 2 : high * 0.9;
        if (next <= low) next = double.IsPositiveInfinity(high) ? Math.Min(1.0, low * 1.05) : (low + high) / 2;
        return next;
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
