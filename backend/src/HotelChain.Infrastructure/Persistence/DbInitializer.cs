using HotelChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HotelChain.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var db = scope.ServiceProvider.GetRequiredService<HotelChainDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));

        if (!db.Database.IsRelational())
            await db.Database.EnsureCreatedAsync(); // InMemory provider (tests) has no migrations.
        else if (options.ApplyMigrations)
            await db.Database.MigrateAsync();

        if (options.Seed && !await db.Hotels.AnyAsync())
        {
            db.Hotels.AddRange(SampleHotels());
            await db.SaveChangesAsync();
            logger.LogInformation("Base de datos inicializada con hoteles de ejemplo.");
        }
    }

    private static IEnumerable<Hotel> SampleHotels()
    {
        var now = DateTime.UtcNow;
        Hotel H(string name, string city, string address, int rating, decimal price, string desc, string img) => new()
        {
            Name = name, City = city, Address = address, Rating = rating, PricePerNight = price,
            Description = desc, ImageUrl = img, CreatedAt = now
        };

        return
        [
            H("Aurora Cartagena", "Cartagena", "Calle del Arsenal 10-45", 5, 780000,
              "Hotel boutique frente a la bahía, con piscina en la terraza y vista a la ciudad amurallada.",
              "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800"),
            H("Aurora Bogotá Centro", "Bogotá", "Carrera 7 # 26-20", 4, 420000,
              "Ubicado en el centro internacional, ideal para viajes de negocios.",
              "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?w=800"),
            H("Aurora Medellín Poblado", "Medellín", "Calle 10 # 43-12", 4, 390000,
              "A pasos del Parque Lleras, con restaurante de cocina local y gimnasio.",
              "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?w=800"),
            H("Aurora Santa Marta", "Santa Marta", "Carrera 1 # 22-60", 3, 260000,
              "Frente a la playa, punto de partida perfecto para el Parque Tayrona.",
              "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800"),
            H("Aurora Cali Express", "Cali", "Avenida 6N # 15-30", 2, 150000,
              "Habitaciones prácticas y económicas en el norte de la ciudad.",
              "https://images.unsplash.com/photo-1590490360182-c33d57733427?w=800"),
            H("Aurora Eje Cafetero", "Pereira", "Km 5 vía Santa Rosa", 5, 650000,
              "Hacienda cafetera restaurada con spa y recorridos por cultivos de café.",
              "https://images.unsplash.com/photo-1571896349842-33c89424de2d?w=800"),
            H("Aurora Barranquilla", "Barranquilla", "Calle 79 # 51-10", 3, 230000,
              "Cerca del malecón del río y de los principales centros comerciales.",
              "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800"),
            H("Aurora Hostal Villa de Leyva", "Villa de Leyva", "Calle 12 # 9-35", 1, 90000,
              "Hostal sencillo y acogedor a dos cuadras de la plaza principal.",
              "https://images.unsplash.com/photo-1555854877-bab0e564b8d5?w=800"),
        ];
    }
}
