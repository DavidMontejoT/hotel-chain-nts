using HotelChain.Domain.Entities;

namespace HotelChain.Application.Hotels;

internal static class HotelMappings
{
    public static HotelDto ToDto(this Hotel h) => new(
        h.Id, h.Name, h.City, h.Address, h.Description, h.Rating, h.PricePerNight, h.ImageUrl, h.CreatedAt, h.UpdatedAt);

    public static void Apply(this Hotel h, HotelRequest r)
    {
        h.Name = r.Name.Trim();
        h.City = r.City.Trim();
        h.Address = r.Address.Trim();
        h.Description = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim();
        h.Rating = r.Rating;
        h.PricePerNight = r.PricePerNight;
        h.ImageUrl = string.IsNullOrWhiteSpace(r.ImageUrl) ? null : r.ImageUrl.Trim();
    }
}
