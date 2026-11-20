using Friendsgiving.Api.Contracts;

namespace Friendsgiving.Api.Hubs;

public interface IFriendsgivingClient
{
    /// <summary>Sent to the gathering's group with its full state after any change.</summary>
    Task GatheringUpdated(GatheringDto gathering);

    Task GatheringDeleted(Guid gatheringId);
}
