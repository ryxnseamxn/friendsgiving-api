using Friendsgiving.Api.Models;

namespace Friendsgiving.Api.Hubs;

public interface IFriendsgivingClient
{
    Task JoinSuccess(string gatheringId);
    Task LeaveSuccess(string gatheringId);
    Task OnDishClaimed(GatheredDishDto dish);
    Task OnDishUnclaimed(string dishItemId);
    Task OnTableUpdated();
}

public record GatheredDishDto
{
    public Guid Id { get; set; }
    public Guid GatheringId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public bool IsClaimed { get; set; }
    public string? ClaimedBy { get; set; }
    public DateTime? ClaimedAt { get; set; }
}