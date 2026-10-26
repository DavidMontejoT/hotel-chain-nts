using HotelChain.Application.Images;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace HotelChain.Infrastructure.Storage;

public static class StaticImagesExtensions
{
    /// <summary>Serves uploaded images from the storage folder under <see cref="ImagePaths.PublicPrefix"/>.</summary>
    public static IApplicationBuilder UseUploadedImages(this IApplicationBuilder app)
    {
        var storage = app.ApplicationServices.GetRequiredService<LocalImageStorage>();
        Directory.CreateDirectory(storage.RootPath);

        return app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.RootPath),
            RequestPath = ImagePaths.PublicPrefix.TrimEnd('/'),
            OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable",
        });
    }
}
