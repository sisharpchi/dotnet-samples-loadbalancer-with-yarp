using MarketplaceSample.Application.Common.Interfaces;
using MarketplaceSample.Domain.Entities;
using MarketplaceSample.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSample.Infrastructure.Products;

internal class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await dbContext.Products.ToListAsync();
    }

    public async Task<long> InsertAsync(Product product)
    {
        await dbContext.Products.AddAsync(product);
        return product.Id;
    }
}
