using HotelChain.Application.Common;
using Microsoft.Extensions.Logging;

namespace HotelChain.Application.Images;

public sealed class ImageService(IImageStorage storage, IImageOptimizer optimizer, ILogger<ImageService> logger) : IImageService
{
    /// <summary>Largest file accepted from the client; it is optimized down to <see cref="TargetBytes"/>.</summary>
    public const long MaxBytes = 10 * 1024 * 1024;

    /// <summary>Maximum weight of a stored image (400 KB).</summary>
    public const long TargetBytes = 400 * 1024;

    public async Task<ImageUploadResult> UploadAsync(Stream content, long length, CancellationToken ct = default)
    {
        if (length == 0)
            throw new ValidationException("El archivo está vacío.");
        if (length > MaxBytes)
            throw new ValidationException("La imagen no puede superar 10 MB.");

        // Trust the file content, not the name or the Content-Type sent by the client.
        var header = new byte[12];
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        var extension = DetectExtension(header.AsSpan(0, read))
            ?? throw new ValidationException("Formato no soportado. Usa una imagen JPG, PNG, WEBP o GIF.");

        using var buffer = new MemoryStream();
        buffer.Write(header, 0, read);
        await content.CopyToAsync(buffer, ct);

        var image = await optimizer.OptimizeAsync(buffer.ToArray(), extension, TargetBytes, ct);
        using var optimized = new MemoryStream(image.Content, writable: false);
        var url = await storage.SaveAsync(optimized, image.Extension, ct);

        logger.LogInformation(
            "Image uploaded to {ImageUrl}: {OriginalBytes} -> {Bytes} bytes, {Width}x{Height}",
            url, length, image.Content.Length, image.Width, image.Height);
        return new ImageUploadResult(url, image.Width, image.Height, image.Content.Length);
    }

    private static string? DetectExtension(ReadOnlySpan<byte> h)
    {
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ".jpg";
        if (h.Length >= 8 && h[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return ".png";
        if (h.Length >= 6 && (h[..6].SequenceEqual("GIF87a"u8) || h[..6].SequenceEqual("GIF89a"u8))) return ".gif";
        if (h.Length >= 12 && h[..4].SequenceEqual("RIFF"u8) && h[8..12].SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }
}
