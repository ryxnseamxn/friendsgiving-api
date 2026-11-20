using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Friendsgiving.Api.Contracts;
using Friendsgiving.Api.Infrastructure;
using Friendsgiving.Api.Services;

namespace Friendsgiving.Api.Controllers;

/// <summary>Guest-facing endpoints: anyone with the gathering link can view, claim and add dishes, and share allergies.</summary>
[ApiController]
[Route("api/gatherings/{gatheringId:guid}")]
public class GatheringsController(GatheringService gatherings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GatheringDto>> Get(Guid gatheringId)
    {
        var gathering = await gatherings.FindAsync(gatheringId);
        return gathering is null ? NotFound("Gathering not found.") : gathering.ToDto();
    }

    [HttpPost("items")]
    public async Task<ActionResult<GatheringDto>> AddItem(Guid gatheringId, AddItemRequest request)
    {
        var name = Input.Clean(request.Name);
        if (name is null) return BadRequest("Dish name is required.");

        var gathering = await gatherings.FindAsync(gatheringId);
        if (gathering is null) return NotFound("Gathering not found.");

        var dish = gathering.AddDish(name, request.Category, Input.Clean(request.Emoji), isCustom: true);
        if (Input.Clean(request.ClaimedBy) is { } claimedBy) dish.ClaimFor(claimedBy, note: null);

        return await gatherings.SaveAsync(gathering);
    }

    [HttpPost("items/{itemId:guid}/claim")]
    public async Task<ActionResult<GatheringDto>> Claim(Guid gatheringId, Guid itemId, ClaimRequest request)
    {
        var guestName = Input.Clean(request.GuestName);
        if (guestName is null) return BadRequest("Enter your name before claiming a dish.");

        var gathering = await gatherings.FindAsync(gatheringId);
        var dish = gathering?.DishItems.FirstOrDefault(d => d.Id == itemId);
        if (gathering is null || dish is null) return NotFound("Dish not found.");
        if (dish.Claim is not null) return Conflict($"{dish.Claim.GuestName} is already bringing {dish.Name}.");

        dish.ClaimFor(guestName, Input.Clean(request.Note));

        try
        {
            return await gatherings.SaveAsync(gathering);
        }
        catch (DbUpdateException)
        {
            // The unique index on Claims.DishItemId caught a simultaneous claim.
            return Conflict($"Someone just claimed {dish.Name}.");
        }
    }

    [HttpDelete("items/{itemId:guid}/claim")]
    public async Task<ActionResult<GatheringDto>> Unclaim(Guid gatheringId, Guid itemId)
    {
        var gathering = await gatherings.FindAsync(gatheringId);
        var dish = gathering?.DishItems.FirstOrDefault(d => d.Id == itemId);
        if (gathering is null || dish is null) return NotFound("Dish not found.");

        gatherings.RemoveClaim(dish);
        return await gatherings.SaveAsync(gathering);
    }

    [HttpPut("allergies")]
    public async Task<ActionResult<GatheringDto>> SetAllergies(Guid gatheringId, AllergyRequest request)
    {
        var guestName = Input.Clean(request.GuestName);
        if (guestName is null) return BadRequest("Enter your name before sharing allergies.");

        var gathering = await gatherings.FindAsync(gatheringId);
        if (gathering is null) return NotFound("Gathering not found.");

        gathering.SetAllergies(guestName, Input.Clean(request.Details));
        return await gatherings.SaveAsync(gathering);
    }
}
