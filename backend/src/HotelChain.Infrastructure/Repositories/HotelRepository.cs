using HotelChain.Domain.Entities;
using HotelChain.Domain.Repositories;
using HotelChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelChain.Infrastructure.Repositories;

public sealed class HotelRepository(HotelChainDbContext db) : EfRepository<Hotel>(db), IHotelRepository
{
    public async Task<(IReadOnlyList<Hotel> Items, int Total)> SearchAsync(HotelSearchCriteria criteria, CancellationToken ct = default)
    {
        IQueryable<Hotel> query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.Name))
        {
            var name = criteria.Name.ToLower();
            query = query.Where(h => h.Name.ToLower().Contains(name));
        }

        if (criteria.Rating.HasValue)
            query = query.Where(h => h.Rating == criteria.Rating.Value);

        if (criteria.MinRating.HasValue)
            query = query.Where(h => h.Rating >= criteria.MinRating.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(h => h.Rating)
            .ThenBy(h => h.Name)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var lowered = name.ToLower();
        return Set.AnyAsync(h => h.Name.ToLower() == lowered && (excludeId == null || h.Id != excludeId), ct);
    }
}
