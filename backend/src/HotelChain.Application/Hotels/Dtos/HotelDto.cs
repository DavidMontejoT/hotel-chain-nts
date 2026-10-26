namespace HotelChain.Application.Hotels.Dtos;

/// <summary>Read model returned by the API.</summary>
public sealed record HotelDto(
    int Id,
    string Name,
    string City,
    string Address,
    string? Description,
    int Rating,
    decimal PricePerNight,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
