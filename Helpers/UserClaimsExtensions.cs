using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public static class UserClaimsExtensions
{
    public static int RestaurantId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst("restaurantId");
        if (claim == null) throw new UnauthorizedAccessException("restaurantId claim missing");
        return int.Parse(claim.Value);
    }

    public static int UserId(this ClaimsPrincipal user)
    {
        var claim =
            user.FindFirst(ClaimTypes.NameIdentifier) ??
            user.FindFirst(JwtRegisteredClaimNames.Sub);

        if (claim == null)
            throw new UnauthorizedAccessException("UserId claim missing");

        return int.Parse(claim.Value);
    }
}
