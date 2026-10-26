using HotelChain.Application.Common;

namespace HotelChain.Application.Hotels;

public interface IHotelService
{
    Task<PagedResult<HotelDto>> SearchAsync(HotelQuery query, CancellationToken ct = default);
    Task<HotelDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<HotelDto> CreateAsync(HotelRequest request, CancellationToken ct = default);
    Task<HotelDto> UpdateAsync(int id, HotelRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
