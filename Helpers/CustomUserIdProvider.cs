using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Billbyte_BE.Helpers
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var user = connection.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Prefer NameIdentifier
            var userId =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("userId")?.Value;

            return userId;
        }
    }
}
