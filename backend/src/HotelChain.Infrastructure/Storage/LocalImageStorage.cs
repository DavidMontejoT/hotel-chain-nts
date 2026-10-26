using HotelChain.Application.Images;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HotelChain.Infrastructure.Storage;

/// <summary>Stores images on the local file system; they are served as static files under /uploads.</summary>
public sealed class LocalImageStorage(IOptions<ImageStorageOptions> options, IHostEnvironment env) : IImageStorage
{
    public string RootPath { get; } = Path.GetFullPath(options.Value.RootPath, env.ContentRootPath);

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct = default)
    {
        Directory.CreateDirectory(RootPath);
        var fileName = $"{Guid.NewGuid():N}{extension}";

        await using var file = File.Create(Path.Combine(RootPath, fileName));
        await content.CopyToAsync(file, ct);

        return ImagePaths.PublicPrefix + fileName;
    }

    public bool IsManaged(string? url)
        => url is not null
           && url.StartsWith(ImagePaths.PublicPrefix, StringComparison.Ordinal)
           && url.Length > ImagePaths.PublicPrefix.Length;

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (IsManaged(url))
        {
            // GetFileName strips any directory part, so a crafted URL cannot escape the folder.
            var path = Path.Combine(RootPath, Path.GetFileName(url));
            if (File.Exists(path)) File.Delete(path);
        }
        return Task.CompletedTask;
    }
}
