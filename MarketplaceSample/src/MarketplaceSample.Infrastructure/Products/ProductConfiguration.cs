using MarketplaceSample.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketplaceSample.Infrastructure.Products;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(128);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(512);
        builder.Property(t => t.ImageUrl).IsRequired(false).HasMaxLength(1028);
    }
}