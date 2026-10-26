using HotelChain.Application.Auth;
using HotelChain.Application.Common;
using HotelChain.Application.Hotels;
using HotelChain.Application.Hotels.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelChain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class HotelsController(IHotelService hotels) : ControllerBase
{
    /// <summary>Lists hotels, optionally filtered by name and/or rating. Filtering runs in the database.</summary>
    /// <example>GET /api/hotels?name=aurora&amp;minRating=4</example>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<HotelDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<HotelDto>> Search([FromQuery] HotelQueryDto query, CancellationToken ct)
        => hotels.SearchAsync(query, ct);

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<HotelDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<HotelDto> GetById(int id, CancellationToken ct) => hotels.GetByIdAsync(id, ct);

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<HotelDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HotelDto>> Create(CreateHotelDto dto, CancellationToken ct)
    {
        var created = await hotels.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<HotelDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<HotelDto> Update(int id, UpdateHotelDto dto, CancellationToken ct)
        => hotels.UpdateAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await hotels.DeleteAsync(id, ct);
        return NoContent();
    }
}
