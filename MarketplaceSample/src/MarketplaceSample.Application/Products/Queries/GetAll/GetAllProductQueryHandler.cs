using AutoMapper;
using MarketplaceSample.Application.Common.Interfaces;
using MediatR;

namespace MarketplaceSample.Application.Products.Queries.GetAll;

public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetAllProductQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetAllAsync();
        return _mapper.Map<ProductDto>(product);
    }
}
