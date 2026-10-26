using HotelChain.Domain.Entities;

namespace HotelChain.Domain.Repositories;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<(IReadOnlyList<Hotel> Items, int Total)> SearchAsync(HotelSearchCriteria criteria, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
}

public sealed record HotelSearchCriteria(
    string? Name,
    int? Rating,
    int? MinRating,
    int Page,
    int PageSize);
