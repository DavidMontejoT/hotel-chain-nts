namespace HotelChain.Application.Images;

/// <summary>Persists uploaded images. The local implementation can be replaced by blob storage.</summary>
public interface IImageStorage
{
    /// <summary>Stores the image and returns its public path (e.g. <c>/uploads/abc.jpg</c>).</summary>
    Task<string> SaveAsync(Stream content, string extension, CancellationToken ct = default);

    /// <summary>True when the URL points to an image stored by this application.</summary>
    bool IsManaged(string? url);

    /// <summary>Deletes a managed image; external URLs are ignored.</summary>
    Task DeleteAsync(string url, CancellationToken ct = default);
}
