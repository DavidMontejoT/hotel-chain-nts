using HotelChain.Application.Common;
using HotelChain.Domain.Entities;
using HotelChain.Domain.Repositories;

namespace HotelChain.Application.Hotels;

public sealed class HotelService(IHotelRepository repository, TimeProvider clock) : IHotelService
{
    public async Task<PagedResult<HotelDto>> SearchAsync(HotelQuery query, CancellationToken ct = default)
    {
        if (query.Rating.HasValue && query.MinRating.HasValue)
            throw new ValidationException("Use 'rating' o 'minRating', no ambos.");

        var criteria = new HotelSearchCriteria(
            string.IsNullOrWhiteSpace(query.Name) ? null : query.Name.Trim(),
            query.Rating,
            query.MinRating,
            query.Page,
            query.PageSize);

        var (items, total) = await repository.SearchAsync(criteria, ct);
        return new PagedResult<HotelDto>(items.Select(h => h.ToDto()).ToList(), total, query.Page, query.PageSize);
    }

    public async Task<HotelDto> GetByIdAsync(int id, CancellationToken ct = default)
        => (await FindAsync(id, ct)).ToDto();

    public async Task<HotelDto> CreateAsync(HotelRequest request, CancellationToken ct = default)
    {
        await EnsureUniqueNameAsync(request.Name, null, ct);

        var hotel = new Hotel { CreatedAt = clock.GetUtcNow().UtcDateTime };
        hotel.Apply(request);

        await repository.AddAsync(hotel, ct);
        return hotel.ToDto();
    }

    public async Task<HotelDto> UpdateAsync(int id, HotelRequest request, CancellationToken ct = default)
    {
        var hotel = await FindAsync(id, ct);
        await EnsureUniqueNameAsync(request.Name, id, ct);

        hotel.Apply(request);
        hotel.UpdatedAt = clock.GetUtcNow().UtcDateTime;

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

    private async Task EnsureUniqueNameAsync(string name, int? excludeId, CancellationToken ct)
    {
        if (await repository.ExistsByNameAsync(name.Trim(), excludeId, ct))
            throw new ConflictException($"Ya existe un hotel con el nombre '{name.Trim()}'.");
    }
}
