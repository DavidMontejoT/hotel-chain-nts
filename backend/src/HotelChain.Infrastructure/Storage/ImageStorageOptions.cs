namespace HotelChain.Infrastructure.Storage;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>Folder for uploaded images; relative paths are resolved from the content root.</summary>
    public string RootPath { get; set; } = "uploads";
}
