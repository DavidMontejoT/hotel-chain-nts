using HotelChain.Application.Common;
using HotelChain.Application.Hotels.Dtos;
using HotelChain.Application.Images;
using HotelChain.Domain.Common;
using HotelChain.Domain.Entities;
using HotelChain.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace HotelChain.Application.Hotels;

public sealed class HotelService(
    IHotelRepository repository,
    IImageStorage images,
    TimeProvider clock,
    ILogger<HotelService> logger) : IHotelService
{
    public async Task<PagedResult<HotelDto>> SearchAsync(HotelQueryDto query, CancellationToken ct = default)
    {
        if (query.Rating.HasValue && query.MinRating.HasValue)
            throw new ValidationException("Use 'rating' o 'minRating', no ambos.");

        var criteria = new HotelSearchCriteria(
            string.IsNullOrWhiteSpace(query.Name) ? null : TextNormalizer.ForSearch(query.Name),
            query.Rating,
            query.MinRating,
            query.Page,
            query.PageSize);

        var (items, total) = await repository.SearchAsync(criteria, ct);
        logger.LogDebug("Hotel search {@Criteria} returned {Count} of {Total}", criteria, items.Count, total);

        return new PagedResult<HotelDto>(items.Select(h => h.ToDto()).ToList(), total, query.Page, query.PageSize);
    }

    public async Task<HotelDto> GetByIdAsync(int id, CancellationToken ct = default)
        => (await FindAsync(id, ct)).ToDto();

    public async Task<HotelDto> CreateAsync(CreateHotelDto dto, CancellationToken ct = default)
    {
        var hotel = new Hotel { CreatedAt = clock.GetUtcNow().UtcDateTime };
        hotel.Apply(dto);
        await EnsureUniqueNameAsync(hotel, ct);

        await repository.AddAsync(hotel, ct);
        logger.LogInformation("Hotel {HotelId} '{HotelName}' created", hotel.Id, hotel.Name);
        return hotel.ToDto();
    }

    public async Task<HotelDto> UpdateAsync(int id, UpdateHotelDto dto, CancellationToken ct = default)
    {
        var hotel = await FindAsync(id, ct);
        var previousImage = hotel.ImageUrl;
        hotel.Apply(dto);
        hotel.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await EnsureUniqueNameAsync(hotel, ct);

        await repository.UpdateAsync(hotel, ct);
        logger.LogInformation("Hotel {HotelId} '{HotelName}' updated", hotel.Id, hotel.Name);

        if (previousImage != hotel.ImageUrl)
            await DeleteImageAsync(previousImage, ct);
        return hotel.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var hotel = await FindAsync(id, ct);
        await repository.DeleteAsync(hotel, ct);
        logger.LogInformation("Hotel {HotelId} '{HotelName}' deleted", hotel.Id, hotel.Name);
        await DeleteImageAsync(hotel.ImageUrl, ct);
    }

    /// <summary>Removes an uploaded image that is no longer referenced. Failures are logged, never thrown.</summary>
    private async Task DeleteImageAsync(string? url, CancellationToken ct)
    {
        if (!images.IsManaged(url)) return;
        try
        {
            await images.DeleteAsync(url!, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete image {ImageUrl}", url);
        }
    }

    private async Task<Hotel> FindAsync(int id, CancellationToken ct)
    {
        var hotel = await repository.GetByIdAsync(id, ct);
        if (hotel is null)
        {
            logger.LogWarning("Hotel {HotelId} not found", id);
            throw new NotFoundException($"No existe un hotel con id {id}.");
        }
        return hotel;
    }

    private async Task EnsureUniqueNameAsync(Hotel hotel, CancellationToken ct)
    {
        var excludeId = hotel.Id == 0 ? (int?)null : hotel.Id;
        if (await repository.ExistsByNormalizedNameAsync(hotel.NormalizedName, excludeId, ct))
            throw new ConflictException($"Ya existe un hotel con el nombre '{hotel.Name}'.");
    }
}
