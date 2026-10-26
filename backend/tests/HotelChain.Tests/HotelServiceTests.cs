using HotelChain.Application.Common;
using HotelChain.Application.Hotels;
using HotelChain.Application.Hotels.Dtos;
using HotelChain.Application.Images;
using HotelChain.Infrastructure.Persistence;
using HotelChain.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelChain.Tests;

public class HotelServiceTests
{
    private readonly FakeImageStorage _images = new();

    private HotelService CreateService()
    {
        var options = new DbContextOptionsBuilder<HotelChainDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new HotelService(
            new HotelRepository(new HotelChainDbContext(options)), _images, TimeProvider.System, NullLogger<HotelService>.Instance);
    }

    private static CreateHotelDto Request(string name, int rating) => new()
    {
        Name = name, City = "Bogotá", Address = "Calle 1 # 2-3", Rating = rating, PricePerNight = 100
    };

    private static UpdateHotelDto UpdateRequest(string name, int rating) => new()
    {
        Name = name, City = "Bogotá", Address = "Calle 1 # 2-3", Rating = rating, PricePerNight = 100
    };

    [Fact]
    public async Task Search_filters_by_partial_name_case_insensitive()
    {
        var service = CreateService();
        await service.CreateAsync(Request("Hotel Andino", 4));
        await service.CreateAsync(Request("Casa del Mar", 3));

        var result = await service.SearchAsync(new HotelQueryDto { Name = "andi" });

        Assert.Single(result.Items);
        Assert.Equal("Hotel Andino", result.Items[0].Name);
    }

    [Fact]
    public async Task Search_filters_by_name_and_exact_rating()
    {
        var service = CreateService();
        await service.CreateAsync(Request("Aurora Uno", 5));
        await service.CreateAsync(Request("Aurora Dos", 3));
        await service.CreateAsync(Request("Otro", 5));

        var result = await service.SearchAsync(new HotelQueryDto { Name = "aurora", Rating = 5 });

        Assert.Equal(1, result.Total);
        Assert.Equal("Aurora Uno", result.Items[0].Name);
    }

    [Fact]
    public async Task Search_filters_by_min_rating_and_orders_by_rating_desc()
    {
        var service = CreateService();
        await service.CreateAsync(Request("A", 2));
        await service.CreateAsync(Request("B", 4));
        await service.CreateAsync(Request("C", 5));

        var result = await service.SearchAsync(new HotelQueryDto { MinRating = 4 });

        Assert.Equal(["C", "B"], result.Items.Select(h => h.Name));
    }

    [Fact]
    public async Task Search_rejects_rating_and_min_rating_together()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SearchAsync(new HotelQueryDto { Rating = 3, MinRating = 2 }));
    }

    [Fact]
    public async Task Create_rejects_duplicate_name()
    {
        var service = CreateService();
        await service.CreateAsync(Request("Hotel Andino", 4));

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(Request("hotel andino", 3)));
    }

    [Fact]
    public async Task Update_and_delete_unknown_hotel_throw_not_found()
    {
        var service = CreateService();
        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(99, UpdateRequest("X", 3)));
        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(99));
    }

    [Fact]
    public async Task Update_changes_fields_and_sets_updated_at()
    {
        var service = CreateService();
        var created = await service.CreateAsync(Request("Hotel Andino", 4));

        var updated = await service.UpdateAsync(created.Id, UpdateRequest("Hotel Andino Plus", 5));

        Assert.Equal("Hotel Andino Plus", updated.Name);
        Assert.Equal(5, updated.Rating);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task Search_ignores_accents_and_case()
    {
        var service = CreateService();
        await service.CreateAsync(Request("Aurora Bogotá Centro", 4));

        var result = await service.SearchAsync(new HotelQueryDto { Name = "BOGOTA" });

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Create_rejects_name_that_only_differs_in_accents()
    {
        var service = CreateService();
        await service.CreateAsync(Request("Hotel Medellín", 4));

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(Request("hotel medellin", 3)));
    }

    [Fact]
    public async Task Update_keeps_its_own_name_without_conflict()
    {
        var service = CreateService();
        var created = await service.CreateAsync(Request("Hotel Andino", 4));

        var updated = await service.UpdateAsync(created.Id, UpdateRequest("Hotel Andino", 5));

        Assert.Equal(5, updated.Rating);
    }

    [Fact]
    public async Task Replacing_or_deleting_an_uploaded_image_removes_the_old_file()
    {
        var service = CreateService();
        var dto = Request("Hotel Andino", 4);
        dto.ImageUrl = "/uploads/first.jpg";
        var created = await service.CreateAsync(dto);

        var update = UpdateRequest("Hotel Andino", 4);
        update.ImageUrl = "/uploads/second.jpg";
        await service.UpdateAsync(created.Id, update);
        await service.DeleteAsync(created.Id);

        Assert.Equal(["/uploads/first.jpg", "/uploads/second.jpg"], _images.Deleted);
    }

    [Fact]
    public async Task External_image_urls_are_never_deleted()
    {
        var service = CreateService();
        var dto = Request("Hotel Andino", 4);
        dto.ImageUrl = "https://example.com/photo.jpg";
        var created = await service.CreateAsync(dto);

        await service.DeleteAsync(created.Id);

        Assert.Empty(_images.Deleted);
    }

    private sealed class FakeImageStorage : IImageStorage
    {
        public List<string> Deleted { get; } = [];

        public Task<string> SaveAsync(Stream content, string extension, CancellationToken ct = default)
            => Task.FromResult(ImagePaths.PublicPrefix + "fake" + extension);

        public bool IsManaged(string? url) => url?.StartsWith(ImagePaths.PublicPrefix) == true;

        public Task DeleteAsync(string url, CancellationToken ct = default)
        {
            Deleted.Add(url);
            return Task.CompletedTask;
        }
    }
}
