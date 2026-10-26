using HotelChain.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelChain.Infrastructure.Persistence;

internal sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> b)
    {
        b.ToTable("Hotels");
        b.HasKey(h => h.Id);
        b.Property(h => h.Name).IsRequired().HasMaxLength(150);
        b.HasIndex(h => h.Name).IsUnique();
        b.Property(h => h.City).IsRequired().HasMaxLength(100);
        b.Property(h => h.Address).IsRequired().HasMaxLength(250);
        b.Property(h => h.Description).HasMaxLength(2000);
        b.Property(h => h.ImageUrl).HasMaxLength(500);
        b.Property(h => h.PricePerNight).HasPrecision(12, 2);
        b.Property(h => h.Rating).IsRequired();
        b.HasIndex(h => h.Rating);
        b.ToTable(t => t.HasCheckConstraint("CK_Hotels_Rating", $"Rating BETWEEN {Hotel.MinRating} AND {Hotel.MaxRating}"));
    }
}
