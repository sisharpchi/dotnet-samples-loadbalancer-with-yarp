using MarketplaceSample.Domain.Entities;

namespace MarketplaceSample.Application.Common.Interfaces;

public interface IProductRepository 
{
    Task<long> Insert(Product product);
}