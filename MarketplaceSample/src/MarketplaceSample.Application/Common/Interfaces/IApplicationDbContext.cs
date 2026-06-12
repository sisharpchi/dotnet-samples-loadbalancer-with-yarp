using MarketplaceSample.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketplaceSample.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}