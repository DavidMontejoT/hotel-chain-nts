namespace HotelChain.Application.Images;

/// <summary>An image ready to be stored, with its final format and dimensions.</summary>
public sealed record OptimizedImage(byte[] Content, string Extension, int Width, int Height);

/// <summary>Fits an image within a byte budget while keeping as much resolution as possible.</summary>
public interface IImageOptimizer
{
    /// <summary>
    /// Returns the original bytes when they already fit in <paramref name="maxBytes"/>; otherwise re-encodes
    /// the image at a fixed high quality and reduces its dimensions only as much as needed to fit.
    /// </summary>
    Task<OptimizedImage> OptimizeAsync(byte[] image, string extension, long maxBytes, CancellationToken ct = default);
}
