using HotelChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HotelChain.Infrastructure.Persistence;

public class HotelChainDbContext(DbContextOptions<HotelChainDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelChainDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        // Dates are stored in UTC and flagged as UTC when read, so they serialize with a trailing "Z".
        builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
}
