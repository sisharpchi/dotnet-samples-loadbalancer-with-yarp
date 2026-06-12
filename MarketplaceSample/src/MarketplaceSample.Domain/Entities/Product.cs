using MarketplaceSample.Domain.Common;

namespace MarketplaceSample.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? ImageUrl { get; set; }
}
