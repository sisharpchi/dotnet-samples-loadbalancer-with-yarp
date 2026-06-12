using MarketplaceSample.Application.Common.Interfaces;
using MarketplaceSample.Domain.Entities;
using MarketplaceSample.Infrastructure.Database;

namespace MarketplaceSample.Infrastructure.Products;

internal class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task<long> Insert(Product product)
    {
        await dbContext.Products.AddAsync(product);
        return product.Id;
    }
}
