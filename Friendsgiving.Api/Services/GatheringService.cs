using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Friendsgiving.Api.Contracts;
using Friendsgiving.Api.Data;
using Friendsgiving.Api.Hubs;
using Friendsgiving.Api.Models;

namespace Friendsgiving.Api.Services;

/// <summary>Loads gatherings and persists changes, broadcasting the result to connected clients.</summary>
public class GatheringService(AppDbContext db, IHubContext<FriendsgivingHub, IFriendsgivingClient> hub)
{
    public Task<Gathering?> FindAsync(Guid id) =>
        db.Gatherings
            .Include(g => g.DishItems).ThenInclude(d => d.Claim)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<GatheringDto> SaveAsync(Gathering gathering)
    {
        await db.SaveChangesAsync();

        var dto = gathering.ToDto();
        await hub.Clients.Group(FriendsgivingHub.GroupName(gathering.Id)).GatheringUpdated(dto);
        return dto;
    }

    public async Task DeleteAsync(Gathering gathering)
    {
        db.Gatherings.Remove(gathering);
        await db.SaveChangesAsync();
        await hub.Clients.Group(FriendsgivingHub.GroupName(gathering.Id)).GatheringDeleted(gathering.Id);
    }

    public void RemoveDish(Gathering gathering, DishItem dish)
    {
        gathering.DishItems.Remove(dish);
        db.DishItems.Remove(dish);
    }

    public void RemoveClaim(DishItem dish)
    {
        if (dish.Claim is null) return;
        db.Claims.Remove(dish.Claim);
        dish.Claim = null;
    }
}
