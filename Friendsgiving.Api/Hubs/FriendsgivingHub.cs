using Microsoft.AspNetCore.SignalR;

namespace Friendsgiving.Api.Hubs;

public class FriendsgivingHub : Hub<IFriendsgivingClient>
{
    public static string GroupName(Guid gatheringId) => $"gathering:{gatheringId}";

    public Task JoinGathering(Guid gatheringId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(gatheringId));
}
