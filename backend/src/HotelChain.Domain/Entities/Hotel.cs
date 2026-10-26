using HotelChain.Domain.Common;

namespace HotelChain.Domain.Entities;

public class Hotel : Entity
{
    public const int MinRating = 1;
    public const int MaxRating = 5;

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            NormalizedName = TextNormalizer.ForSearch(value);
        }
    }

    /// <summary>Lower-case, accent-free copy of <see cref="Name"/> used for searching and uniqueness.</summary>
    public string NormalizedName { get; private set; } = string.Empty;

    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Star rating from 1 to 5.</summary>
    public int Rating { get; set; }

    public decimal PricePerNight { get; set; }
    public string? ImageUrl { get; set; }
}
