using MarketplaceSample.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace MarketplaceSample.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<IdentityErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new IdentityErrorResponse(result.Errors));
        }

        return Ok(UserResponse.FromUser(user));
    }

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        return user is null ? NotFound() : Ok(UserResponse.FromUser(user));
    }
}

public record RegisterUserRequest
{
    public required string UserName { get; init; }

    public string? Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public required string Password { get; init; }
}

public record UserResponse(string Id, string? UserName, string? Email, string? FirstName, string? LastName)
{
    public static UserResponse FromUser(ApplicationUser user)
    {
        return new UserResponse(user.Id, user.UserName, user.Email, user.FirstName, user.LastName);
    }
}

public record IdentityErrorResponse(IEnumerable<IdentityError> Errors);
