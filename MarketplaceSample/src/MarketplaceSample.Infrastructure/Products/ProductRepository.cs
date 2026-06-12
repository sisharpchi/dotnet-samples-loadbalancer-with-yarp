using MarketplaceSample.Domain.Entities;
using MarketplaceSample.Infrastructure.Database;

namespace MarketplaceSample.Infrastructure.Products;

internal class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public void Insert(Product product)
    {
        dbContext.Products.Add(product);
    }
}
