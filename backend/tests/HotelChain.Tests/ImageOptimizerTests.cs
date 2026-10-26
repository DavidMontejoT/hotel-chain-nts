using HotelChain.Application.Common;
using HotelChain.Application.Images;
using HotelChain.Infrastructure.Storage;
using SkiaSharp;

namespace HotelChain.Tests;

public class ImageOptimizerTests
{
    private const long MaxBytes = 400 * 1024;
    private readonly SkiaImageOptimizer _optimizer = new();

    [Fact]
    public async Task Large_images_are_reduced_to_the_byte_budget_keeping_the_aspect_ratio()
    {
        var original = TestImages.LargePng();
        Assert.True(original.Length > MaxBytes);

        var result = await _optimizer.OptimizeAsync(original, ".png", MaxBytes);

        Assert.True(result.Content.Length <= MaxBytes, $"{result.Content.Length} bytes");
        Assert.Equal(".webp", result.Extension);
        Assert.Equal(2400.0 / 1600, (double)result.Width / result.Height, precision: 2);

        using var stored = SKBitmap.Decode(result.Content);
        Assert.Equal(result.Width, stored.Width);
    }

    [Fact]
    public async Task The_result_keeps_close_to_the_largest_resolution_that_fits()
    {
        var result = await _optimizer.OptimizeAsync(TestImages.LargePng(), ".png", MaxBytes);

        // An image 10% larger at the same quality no longer fits, so resolution was not given away.
        var bigger = TestImages.WebpSize(TestImages.LargePng(), (int)(result.Width * 1.1), (int)(result.Height * 1.1), SkiaImageOptimizer.Quality);

        Assert.True(bigger > MaxBytes);
    }

    [Fact]
    public async Task Images_within_the_budget_are_kept_untouched()
    {
        var small = TestImages.LargePng(200, 120);
        Assert.True(small.Length <= MaxBytes);

        var result = await _optimizer.OptimizeAsync(small, ".png", MaxBytes);

        Assert.Same(small, result.Content);
        Assert.Equal(".png", result.Extension);
        Assert.Equal((200, 120), (result.Width, result.Height));
    }

    [Fact]
    public async Task Corrupt_images_are_rejected()
    {
        byte[] corrupt = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];
        await Assert.ThrowsAsync<ValidationException>(() => _optimizer.OptimizeAsync(corrupt, ".png", MaxBytes));
    }
}
