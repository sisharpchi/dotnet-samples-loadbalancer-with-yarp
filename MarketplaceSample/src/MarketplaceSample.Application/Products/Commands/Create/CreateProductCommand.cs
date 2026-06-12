using MediatR;

namespace MarketplaceSample.Application.Products.Commands.Create;

public class CreateProductCommand : IRequest<long>
{
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public string? ImageUrl { get; init; }
}
