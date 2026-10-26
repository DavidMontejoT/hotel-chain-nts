using HotelChain.Application.Common;
using HotelChain.Application.Hotels.Dtos;
using HotelChain.Domain.Common;
using HotelChain.Domain.Entities;
using HotelChain.Domain.Repositories;

namespace HotelChain.Application.Hotels;

public sealed class HotelService(IHotelRepository repository, TimeProvider clock) : IHotelService
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
        return hotel.ToDto();
    }

    public async Task<HotelDto> UpdateAsync(int id, UpdateHotelDto dto, CancellationToken ct = default)
    {
        var hotel = await FindAsync(id, ct);
        hotel.Apply(dto);
        hotel.UpdatedAt = clock.GetUtcNow().UtcDateTime;
        await EnsureUniqueNameAsync(hotel, ct);

        await repository.UpdateAsync(hotel, ct);
        return hotel.ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var hotel = await FindAsync(id, ct);
        await repository.DeleteAsync(hotel, ct);
    }

    private async Task<Hotel> FindAsync(int id, CancellationToken ct)
        => await repository.GetByIdAsync(id, ct)
           ?? throw new NotFoundException($"No existe un hotel con id {id}.");

    private async Task EnsureUniqueNameAsync(Hotel hotel, CancellationToken ct)
    {
        var excludeId = hotel.Id == 0 ? (int?)null : hotel.Id;
        if (await repository.ExistsByNormalizedNameAsync(hotel.NormalizedName, excludeId, ct))
            throw new ConflictException($"Ya existe un hotel con el nombre '{hotel.Name}'.");
    }
}
