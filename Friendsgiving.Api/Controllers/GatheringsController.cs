using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Friendsgiving.Api.Data;
using Friendsgiving.Api.Hubs;
using Friendsgiving.Api.Models;

namespace Friendsgiving.Api.Controllers;

[ApiController]
[Route("api/gatherings")]
public class GatheringsController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly IHubContext<FriendsgivingHub, IFriendsgivingClient> _hub;

    public GatheringsController(AppDbContext ctx, IHubContext<FriendsgivingHub, IFriendsgivingClient> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    // GET /api/gatherings/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GatheringDto>> Get(Guid id)
    {
        var gathering = await _ctx.Gatherings
            .Include(g => g.DishItems).ThenInclude(d => d.Claim)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gathering == null) return NotFound();

        var dto = new GatheringDto
        {
            Id = gathering.Id,
            Name = gathering.Name,
            Date = gathering.Date,
            HostName = gathering.HostName,
            Location = gathering.Location,
            Items = gathering.DishItems.Select(d => new ItemDto
            {
                Id = d.Id,
                Name = d.Name,
                Category = d.Category,
                Emoji = d.Emoji,
                IsCustom = d.IsCustom,
                SortOrder = d.SortOrder,
                IsClaimed = d.Claim != null,
                ClaimedBy = d.Claim?.GuestName,
                ClaimedAt = d.Claim?.ClaimedAt,
            }).ToList()
        };

        return Ok(dto);
    }

    // POST /api/gatherings/{id}/items
    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult<ItemDto>> AddItem(Guid id, [FromBody] AddItemRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Dish name is required.");

        var gathering = await _ctx.Gatherings
            .Include(g => g.DishItems)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gathering == null) return NotFound("Gathering not found");

        var dish = new DishItem
        {
            Id = Guid.NewGuid(),
            GatheringId = gathering.Id,
            Name = request.Name,
            Category = string.IsNullOrWhiteSpace(request.Category) ? "other" : request.Category,
            Emoji = string.IsNullOrWhiteSpace(request.Emoji) ? "🍽️" : request.Emoji,
            IsCustom = true,
            SortOrder = gathering.DishItems.Count + 1,
        };

        _ctx.DishItems.Add(dish);
        await _ctx.SaveChangesAsync();

        await _hub.Clients.Group(id.ToString()).OnTableUpdated();

        return new ItemDto
        {
            Id = dish.Id,
            Name = dish.Name,
            Category = dish.Category,
            Emoji = dish.Emoji,
            IsCustom = dish.IsCustom,
            SortOrder = dish.SortOrder,
            IsClaimed = false,
            ClaimedBy = null,
            ClaimedAt = null,
        };
    }

    // POST /api/gatherings/{id}/items/{itemId}/claim
    [HttpPost("{id:guid}/items/{itemId:guid}/claim")]
    public async Task<ActionResult<ItemDto>> Claim(Guid id, Guid itemId, [FromBody] ClaimRequest request)
    {
        var gathering = await _ctx.Gatherings
            .Include(g => g.DishItems).ThenInclude(d => d.Claim)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gathering == null) return NotFound("Gathering not found");

        var dish = gathering.DishItems.FirstOrDefault(d => d.Id == itemId);
        if (dish == null) return NotFound("Dish not found in this gathering");

        // If already claimed, return current state
        if (dish.Claim != null)
        {
            return new ItemDto
            {
                Id = dish.Id,
                Name = dish.Name,
                IsClaimed = true,
                ClaimedBy = dish.Claim.GuestName,
                ClaimedAt = dish.Claim.ClaimedAt,
            };
        }

        // Claim it
        var claim = new Claim
        {
            Id = Guid.NewGuid(),
            DishItemId = dish.Id,
            GuestName = request.GuestName,
            Note = request.Note,
            ClaimedAt = DateTime.UtcNow,
        };

        dish.Claim = claim;

        _ctx.Claims.Add(claim);
        await _ctx.SaveChangesAsync();

        // Broadcast via SignalR
        var dishDto = new GatheredDishDto
        {
            Id = dish.Id,
            GatheringId = dish.GatheringId,
            Name = dish.Name,
            Emoji = dish.Emoji,
            IsClaimed = true,
            ClaimedBy = request.GuestName,
            ClaimedAt = claim.ClaimedAt,
        };

        await _hub.Clients.Group(id.ToString()).OnDishClaimed(dishDto);
        await _hub.Clients.Group(id.ToString()).OnTableUpdated();

        return new ItemDto
        {
            Id = dish.Id,
            Name = dish.Name,
            IsClaimed = true,
            ClaimedBy = request.GuestName,
            ClaimedAt = claim.ClaimedAt,
        };
    }

    // DELETE /api/gatherings/{id}/items/{itemId}/claim
    [HttpDelete("{id:guid}/items/{itemId:guid}/claim")]
    public async Task<ActionResult<ItemDto>> Unclaim(Guid id, Guid itemId)
    {
        var gathering = await _ctx.Gatherings
            .Include(g => g.DishItems).ThenInclude(d => d.Claim)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gathering == null) return NotFound("Gathering not found");

        var dish = gathering.DishItems.FirstOrDefault(d => d.Id == itemId);
        if (dish == null) return NotFound("Dish not found");

        // Remove any associated claim
        var existingClaim = await _ctx.Claims.FirstOrDefaultAsync(c => c.DishItemId == dish.Id);
        if (existingClaim != null)
        {
            _ctx.Claims.Remove(existingClaim);
        }

        dish.Claim = null;
        await _ctx.SaveChangesAsync();

        // Broadcast via SignalR
        await _hub.Clients.Group(id.ToString()).OnDishUnclaimed(dish.Id.ToString());
        await _hub.Clients.Group(id.ToString()).OnTableUpdated();

        return new ItemDto
        {
            Id = dish.Id,
            Name = dish.Name,
            IsClaimed = false,
            ClaimedBy = null,
            ClaimedAt = null,
        };
    }
}

public record ClaimRequest
{
    public string GuestName { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public record AddItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "other";
    public string Emoji { get; set; } = "🍽️";
}

public record GatheringDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public List<ItemDto> Items { get; set; } = new();
}

public record ItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public int SortOrder { get; set; }
    public bool IsClaimed { get; set; }
    public string? ClaimedBy { get; set; }
    public DateTime? ClaimedAt { get; set; }
}