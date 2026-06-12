using FluentValidation;
using MarketplaceSample.Application.Common.Constants.Messages;

namespace MarketplaceSample.Application.Products.Commands.Create;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ProductMessage.NameRequired)
            .NotNull().WithMessage(ProductMessage.NameRequired)
            .MaximumLength(128).WithMessage(ProductMessage.NameExceeded);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(ProductMessage.DescriptionRequired)
            .NotNull().WithMessage(ProductMessage.DescriptionRequired)
            .MaximumLength(512).WithMessage(ProductMessage.DescriptionExceeded);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(1028).WithMessage(ProductMessage.ImageUrlExceeded);
    }
}
