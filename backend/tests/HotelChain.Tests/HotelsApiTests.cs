using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HotelChain.Application.Auth;
using HotelChain.Application.Common;
using HotelChain.Application.Hotels.Dtos;
using HotelChain.Application.Images;
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
            builder.UseSetting("ImageStorage:RootPath", ImagesPath);
        }

        public string ImagesPath { get; } = Path.Combine(Path.GetTempPath(), "hotelchain-tests-" + Guid.NewGuid());

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Directory.Exists(ImagesPath)) Directory.Delete(ImagesPath, recursive: true);
        }
    }

    // Smallest valid PNG (1x1 transparent pixel).
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=");

    private static MultipartFormDataContent FileForm(byte[] content, string fileName, string contentType)
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", fileName } };
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

    [Fact]
    public async Task Admin_can_upload_an_image_and_use_it_in_a_hotel()
    {
        var client = await AdminClientAsync();

        var upload = await client.PostAsync("/api/images", FileForm(Png, "foto.png", "image/png"));
        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var url = (await upload.Content.ReadFromJsonAsync<ImageUploadResult>())!.Url;
        Assert.StartsWith("/uploads/", url);
        Assert.EndsWith(".png", url);

        var image = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, image.StatusCode);
        Assert.Equal("image/png", image.Content.Headers.ContentType?.MediaType);

        var create = await client.PostAsJsonAsync("/api/hotels", new CreateHotelDto
        {
            Name = "Hotel Con Foto", City = "Cali", Address = "Calle 5", Rating = 4, PricePerNight = 100, ImageUrl = url
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var hotel = (await create.Content.ReadFromJsonAsync<HotelDto>())!;

        await client.DeleteAsync($"/api/hotels/{hotel.Id}");
        Assert.False(File.Exists(Path.Combine(_factory.ImagesPath, Path.GetFileName(url))));
    }

    [Fact]
    public async Task Uploaded_photos_are_optimized_to_400_kb_or_less()
    {
        var client = await AdminClientAsync();

        var upload = await client.PostAsync("/api/images", FileForm(TestImages.LargePng(), "grande.png", "image/png"));

        Assert.Equal(HttpStatusCode.Created, upload.StatusCode);
        var result = (await upload.Content.ReadFromJsonAsync<ImageUploadResult>())!;
        Assert.EndsWith(".webp", result.Url);
        Assert.True(result.SizeBytes <= ImageService.TargetBytes);
        Assert.True(new FileInfo(Path.Combine(_factory.ImagesPath, Path.GetFileName(result.Url))).Length <= ImageService.TargetBytes);
    }

    [Fact]
    public async Task Non_image_files_are_rejected_even_with_an_image_content_type()
    {
        var client = await AdminClientAsync();
        var response = await client.PostAsync("/api/images", FileForm("hola"u8.ToArray(), "foto.png", "image/png"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Uploading_images_requires_admin_token()
    {
        var response = await _factory.CreateClient().PostAsync("/api/images", FileForm(Png, "foto.png", "image/png"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Hotel_image_must_be_http_url_or_uploaded_path()
    {
        var client = await AdminClientAsync();
        var response = await client.PostAsJsonAsync("/api/hotels", new CreateHotelDto
        {
            Name = "Hotel Ruta Rara", City = "Cali", Address = "Calle 5", Rating = 4, PricePerNight = 100, ImageUrl = "/uploads/../appsettings.json"
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
