using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HotelChain.Application.Auth;
using HotelChain.Application.Common;
using HotelChain.Application.Hotels.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HotelChain.Tests;

public class HotelsApiTests : IClassFixture<HotelsApiTests.ApiFactory>
{
    public sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Database:Provider", "InMemory");
            builder.UseSetting("ConnectionStrings:HotelChain", "api-tests-" + Guid.NewGuid());
            builder.UseSetting("Database:Seed", "true");
            builder.UseSetting("Jwt:Key", "test-signing-key-0123456789abcdef0123456789");
            builder.UseSetting("AdminUser:Username", "admin");
            builder.UseSetting("AdminUser:Password", "secret");
        }
    }

    private readonly ApiFactory _factory;

    public HotelsApiTests(ApiFactory factory) => _factory = factory;

    private async Task<HttpClient> AdminClientAsync()
    {
        var client = _factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "secret" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Anonymous_users_can_list_and_filter_hotels()
    {
        var client = _factory.CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<HotelDto>>("/api/hotels?minRating=4");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, h => Assert.True(h.Rating >= 4));
    }

    [Fact]
    public async Task Invalid_rating_filter_returns_bad_request()
    {
        var response = await _factory.CreateClient().GetAsync("/api/hotels?rating=9");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Writes_require_admin_token()
    {
        var response = await _factory.CreateClient().DeleteAsync("/api/hotels/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Wrong_password_is_rejected()
    {
        var response = await _factory.CreateClient()
            .PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "nope" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_can_create_update_and_delete_a_hotel()
    {
        var client = await AdminClientAsync();
        var request = new CreateHotelDto { Name = "Hotel de Prueba", City = "Cali", Address = "Calle 5", Rating = 3, PricePerNight = 120 };

        var create = await client.PostAsJsonAsync("/api/hotels", request);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = (await create.Content.ReadFromJsonAsync<HotelDto>())!;

        var update = await client.PutAsJsonAsync($"/api/hotels/{created.Id}", new UpdateHotelDto
        {
            Name = request.Name, City = request.City, Address = request.Address, Rating = 4, PricePerNight = 120
        });
        Assert.Equal(4, (await update.Content.ReadFromJsonAsync<HotelDto>())!.Rating);

        var delete = await client.DeleteAsync($"/api/hotels/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await client.GetAsync($"/api/hotels/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Invalid_hotel_payload_returns_bad_request()
    {
        var client = await AdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/hotels", new { name = "", rating = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
