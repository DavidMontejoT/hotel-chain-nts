using System.ComponentModel.DataAnnotations;

namespace HotelChain.Application.Hotels;

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

/// <summary>Datos de entrada para crear o actualizar un hotel.</summary>
public sealed class HotelRequest
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

    [Url, StringLength(500)]
    public string? ImageUrl { get; set; }
}

/// <summary>Filtros para listar hoteles. Todos son opcionales y combinables.</summary>
public sealed class HotelQuery
{
    /// <summary>Búsqueda parcial por nombre (no distingue mayúsculas).</summary>
    [StringLength(150)]
    public string? Name { get; set; }

    /// <summary>Calificación exacta (1 a 5).</summary>
    [Range(1, 5)]
    public int? Rating { get; set; }

    /// <summary>Calificación mínima (1 a 5).</summary>
    [Range(1, 5)]
    public int? MinRating { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
