using HotelChain.Application.Common;
using HotelChain.Application.Hotels.Dtos;

namespace HotelChain.Application.Hotels;

public interface IHotelService
{
    Task<PagedResult<HotelDto>> SearchAsync(HotelQueryDto query, CancellationToken ct = default);
    Task<HotelDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<HotelDto> CreateAsync(CreateHotelDto dto, CancellationToken ct = default);
    Task<HotelDto> UpdateAsync(int id, UpdateHotelDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
