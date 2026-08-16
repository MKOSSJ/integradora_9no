using Microsoft.AspNetCore.SignalR;

namespace Plandi.Services.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var usuarioIdValue = Context.GetHttpContext()?.Request.Query["usuarioId"].ToString();

        if (long.TryParse(usuarioIdValue, out var usuarioId) && usuarioId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{usuarioId}");
        }

        await base.OnConnectedAsync();
    }
}
