using System.ComponentModel.DataAnnotations;
using HotelChain.Application.Common.Validation;

namespace HotelChain.Application.Hotels.Dtos;

/// <summary>Fields and validation rules shared by the create and update payloads.</summary>
public abstract class HotelWriteDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(250, MinimumLength = 3)]
    public string Address { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Range(0, 100_000_000)]
    public decimal PricePerNight { get; set; }

    /// <summary>External http(s) URL or the path returned by <c>POST /api/images</c>.</summary>
    [ImageUrl, StringLength(500)]
    public string? ImageUrl { get; set; }
}
