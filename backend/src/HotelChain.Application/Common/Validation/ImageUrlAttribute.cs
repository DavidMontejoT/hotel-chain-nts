using System.ComponentModel.DataAnnotations;
using HotelChain.Application.Images;

namespace HotelChain.Application.Common.Validation;

/// <summary>Accepts an absolute http(s) URL or the path of an image uploaded to this API.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ImageUrlAttribute : ValidationAttribute
{
    public ImageUrlAttribute() : base("La imagen debe ser una URL http(s) o una imagen subida.") { }

    public override bool IsValid(object? value)
    {
        if (value is not string url || string.IsNullOrWhiteSpace(url)) return true;

        if (url.StartsWith(ImagePaths.PublicPrefix, StringComparison.Ordinal))
        {
            var fileName = url[ImagePaths.PublicPrefix.Length..];
            return fileName.Length > 0 && fileName == Path.GetFileName(fileName);
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
