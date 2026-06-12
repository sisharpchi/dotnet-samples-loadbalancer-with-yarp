using MarketplaceSample.Application.Common.Interfaces;
using MarketplaceSample.Domain.Entities;
using MediatR;

namespace MarketplaceSample.Application.Products.Commands.Create;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, long>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product()
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl
        };

        await _productRepository.Insert(product);

        return product.Id;
    }

}
