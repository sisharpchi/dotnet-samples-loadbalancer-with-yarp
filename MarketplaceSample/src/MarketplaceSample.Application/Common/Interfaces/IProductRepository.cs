using MarketplaceSample.Domain.Entities;

namespace MarketplaceSample.Application.Common.Interfaces;

public interface IProductRepository 
{
    Task<long> InsertAsync(Product product);

    Task<IEnumerable<Product>> GetAllAsync();
}