using HotelChain.Domain.Entities;
using HotelChain.Domain.Repositories;
using HotelChain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HotelChain.Infrastructure.Repositories;

public sealed class HotelRepository(HotelChainDbContext db) : EfRepository<Hotel>(db), IHotelRepository
{
    public async Task<(IReadOnlyList<Hotel> Items, int Total)> SearchAsync(HotelSearchCriteria criteria, CancellationToken ct = default)
    {
        // Filters are composed on IQueryable so they are translated to SQL and run in the database.
        IQueryable<Hotel> query = Set.AsNoTracking();

        if (!string.IsNullOrEmpty(criteria.NormalizedName))
            query = query.Where(h => h.NormalizedName.Contains(criteria.NormalizedName));

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

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, int? excludeId = null, CancellationToken ct = default)
        => Set.AnyAsync(h => h.NormalizedName == normalizedName && (excludeId == null || h.Id != excludeId), ct);
}
