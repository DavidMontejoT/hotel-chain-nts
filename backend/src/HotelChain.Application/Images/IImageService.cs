namespace HotelChain.Application.Images;

/// <summary>Public URL of the stored image plus its final dimensions and size in bytes.</summary>
public sealed record ImageUploadResult(string Url, int Width, int Height, long SizeBytes);

public interface IImageService
{
    Task<ImageUploadResult> UploadAsync(Stream content, long length, CancellationToken ct = default);
}
