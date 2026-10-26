namespace HotelChain.Application.Images;

public sealed record ImageUploadResult(string Url);

public interface IImageService
{
    Task<ImageUploadResult> UploadAsync(Stream content, long length, CancellationToken ct = default);
}
