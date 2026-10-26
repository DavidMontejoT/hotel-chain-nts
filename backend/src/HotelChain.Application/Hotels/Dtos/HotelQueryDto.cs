using System.ComponentModel.DataAnnotations;

namespace HotelChain.Application.Hotels.Dtos;

/// <summary>Query-string filters for listing hotels. All of them are optional and can be combined.</summary>
public sealed class HotelQueryDto
{
    /// <summary>Partial match on the hotel name, ignoring case and accents.</summary>
    [StringLength(150)]
    public string? Name { get; set; }

    /// <summary>Exact rating (1 to 5).</summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>Minimum rating (1 to 5).</summary>
    [Range(1, 5)]
    public int? MinRating { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
