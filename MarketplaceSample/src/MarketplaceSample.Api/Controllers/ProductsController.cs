using MarketplaceSample.Application.Products.Commands.Create;
using MarketplaceSample.Application.Products.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace MarketplaceSample.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var result = await mediator.Send(new GetAllProductQuery());
        return Ok(result);
    }
}
