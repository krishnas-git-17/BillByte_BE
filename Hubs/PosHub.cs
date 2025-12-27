using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BillByte.Hubs
{
    [Authorize]
    public class PosHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var restaurantId = Context.User!
                .FindFirst("restaurantId")!
                .Value;

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                restaurantId
            );

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var restaurantId = Context.User!
                .FindFirst("restaurantId")!
                .Value;

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                restaurantId
            );

            await base.OnDisconnectedAsync(exception);
        }
    }
}
