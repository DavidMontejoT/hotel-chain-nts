using HotelChain.Application.Hotels.Dtos;
using HotelChain.Domain.Entities;

namespace HotelChain.Application.Hotels;

internal static class HotelMappings
{
    public static HotelDto ToDto(this Hotel h) => new(
        h.Id, h.Name, h.City, h.Address, h.Description, h.Rating, h.PricePerNight, h.ImageUrl, h.CreatedAt, h.UpdatedAt);

    public static void Apply(this Hotel h, HotelWriteDto dto)
    {
        h.Name = dto.Name.Trim();
        h.City = dto.City.Trim();
        h.Address = dto.Address.Trim();
        h.Description = NullIfBlank(dto.Description);
        h.Rating = dto.Rating;
        h.PricePerNight = dto.PricePerNight;
        h.ImageUrl = NullIfBlank(dto.ImageUrl);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
