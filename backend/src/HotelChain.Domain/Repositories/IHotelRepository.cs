using HotelChain.Domain.Entities;

namespace HotelChain.Domain.Repositories;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<(IReadOnlyList<Hotel> Items, int Total)> SearchAsync(HotelSearchCriteria criteria, CancellationToken ct = default);
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, int? excludeId = null, CancellationToken ct = default);
}

/// <param name="NormalizedName">Search term already normalized with <see cref="Common.TextNormalizer.ForSearch"/>.</param>
public sealed record HotelSearchCriteria(
    string? NormalizedName,
    int? Rating,
    int? MinRating,
    int Page,
    int PageSize);
