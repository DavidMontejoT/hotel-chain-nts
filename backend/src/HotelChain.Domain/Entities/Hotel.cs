using HotelChain.Domain.Common;

namespace HotelChain.Domain.Entities;

public class Hotel : Entity
{
    public const int MinRating = 1;
    public const int MaxRating = 5;

    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Calificación del hotel en estrellas (1 a 5).</summary>
    public int Rating { get; set; }

    public decimal PricePerNight { get; set; }
    public string? ImageUrl { get; set; }
}
