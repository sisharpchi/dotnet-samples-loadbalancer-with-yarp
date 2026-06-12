using MarketplaceSample.Application.Products.Commands.Create;
using MarketplaceSample.Application.Products.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MarketplaceSample.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
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
