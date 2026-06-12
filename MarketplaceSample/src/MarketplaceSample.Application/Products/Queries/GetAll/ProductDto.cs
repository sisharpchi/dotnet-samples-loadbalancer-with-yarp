using MarketplaceSample.Application.Common.Interfaces;

namespace MarketplaceSample.Application.Products.Queries.GetAll;

public class ProductDto : IDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? ImageUrl { get; set; }
}
