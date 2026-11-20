using Microsoft.AspNetCore.SignalR;
using Friendsgiving.Api.Hubs;

namespace Friendsgiving.Api.Hubs;

public class FriendsgivingHub : Hub<IFriendsgivingClient>
{
    public async Task JoinGathering(string gatheringId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gatheringId);
        await Clients.Caller.JoinSuccess(gatheringId);
    }

    public async Task LeaveGathering(string gatheringId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gatheringId);
        await Clients.Caller.LeaveSuccess(gatheringId);
    }
}