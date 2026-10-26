using HotelChain.Application.Auth;
using HotelChain.Application.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelChain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = Roles.Admin)]
public sealed class ImagesController(IImageService images) : ControllerBase
{
    /// <summary>Uploads a hotel photo (JPG, PNG, WEBP or GIF, up to 5 MB) and returns the URL to store in the hotel.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(ImageService.MaxBytes + 64 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageService.MaxBytes + 64 * 1024)]
    [ProducesResponseType<ImageUploadResult>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImageUploadResult>> Upload(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        var result = await images.UploadAsync(stream, file.Length, ct);
        return Created(result.Url, result);
    }
}
