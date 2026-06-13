using MarketplaceSample.Domain.Entities.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace MarketplaceSample.Api.Controllers;

[ApiController]
[Route("/security/oauth")]
public class OAuthAuthorizationController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict request cannot be resolved.");

        if (request.IsPasswordGrantType())
        {
            return await ExchangePasswordAsync(request);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await ExchangeRefreshTokenAsync();
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = Errors.UnsupportedGrantType,
            ErrorDescription = "The specified grant type is not supported."
        });
    }

    private async Task<IActionResult> ExchangePasswordAsync(OpenIddictRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "Username and password are required."
            });
        }

        var user = await userManager.FindByNameAsync(request.Username);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The username/password couple is invalid."
            });
        }

        var scopes = new HashSet<string>(request.GetScopes(), StringComparer.Ordinal)
        {
            "api",
            Scopes.OfflineAccess
        };

        var principal = CreatePrincipal(user, scopes);
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeRefreshTokenAsync()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        var principal = result.Principal;

        if (principal is null || principal.Identity?.IsAuthenticated != true)
        {
            return Unauthorized(new OpenIddictResponse
            {
                Error = Errors.InvalidGrant,
                ErrorDescription = "The refresh token is invalid."
            });
        }

        var userId = principal.GetClaim(Claims.Subject);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var refreshedPrincipal = CreatePrincipal(user, principal.GetScopes());
        return SignIn(refreshedPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static ClaimsPrincipal CreatePrincipal(ApplicationUser user, IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            Claims.Name,
            Claims.Role);

        identity.SetClaim(Claims.Subject, user.Id);
        identity.SetClaim(Claims.Name, user.UserName);
        identity.SetClaim(ClaimTypes.NameIdentifier, user.Id);

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            identity.SetClaim(Claims.Email, user.Email);
        }

        if (!string.IsNullOrWhiteSpace(user.FirstName))
        {
            identity.SetClaim(Claims.GivenName, user.FirstName);
        }

        if (!string.IsNullOrWhiteSpace(user.LastName))
        {
            identity.SetClaim(Claims.FamilyName, user.LastName);
        }

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes);
        principal.SetDestinations(GetDestinations);

        return principal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            Claims.Subject or Claims.Name or Claims.Email or Claims.GivenName or Claims.FamilyName
                => [Destinations.AccessToken, Destinations.IdentityToken],
            _ => [Destinations.AccessToken]
        };
    }
}
