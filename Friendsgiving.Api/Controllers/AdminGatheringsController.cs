using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Friendsgiving.Api.Contracts;
using Friendsgiving.Api.Data;
using Friendsgiving.Api.Infrastructure;
using Friendsgiving.Api.Models;
using Friendsgiving.Api.Services;

namespace Friendsgiving.Api.Controllers;

/// <summary>Host-facing endpoints for creating gatherings and curating their dish lists.</summary>
[ApiController]
[AdminKey]
[Route("api/admin/gatherings")]
public class AdminGatheringsController(AppDbContext db, GatheringService gatherings) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<GatheringSummaryDto>> List() =>
        await db.Gatherings
            .OrderByDescending(g => g.Date)
            .Select(g => new GatheringSummaryDto(
                g.Id,
                g.Name,
                g.Date,
                g.HostName,
                g.Location,
                g.DishItems.Count,
                g.DishItems.Count(d => d.Claim != null)))
            .ToListAsync();

    [HttpPost]
    public async Task<ActionResult<GatheringDto>> Create(GatheringRequest request)
    {
        var gathering = new Gathering();
        if (Apply(gathering, request) is { } error) return BadRequest(error);

        db.Gatherings.Add(gathering);
        var dto = await gatherings.SaveAsync(gathering);
        return Created($"/api/gatherings/{dto.Id}", dto);
    }

    [HttpPut("{gatheringId:guid}")]
    public async Task<ActionResult<GatheringDto>> Update(Guid gatheringId, GatheringRequest request)
    {
        var gathering = await gatherings.FindAsync(gatheringId);
        if (gathering is null) return NotFound("Gathering not found.");
        if (Apply(gathering, request) is { } error) return BadRequest(error);

        return await gatherings.SaveAsync(gathering);
    }

    [HttpDelete("{gatheringId:guid}")]
    public async Task<IActionResult> Delete(Guid gatheringId)
    {
        var gathering = await gatherings.FindAsync(gatheringId);
        if (gathering is null) return NotFound("Gathering not found.");

        await gatherings.DeleteAsync(gathering);
        return NoContent();
    }

    [HttpPost("{gatheringId:guid}/items")]
    public async Task<ActionResult<GatheringDto>> AddItem(Guid gatheringId, AddItemRequest request)
    {
        var name = Input.Clean(request.Name);
        if (name is null) return BadRequest("Dish name is required.");

        var gathering = await gatherings.FindAsync(gatheringId);
        if (gathering is null) return NotFound("Gathering not found.");

        gathering.AddDish(name, request.Category, Input.Clean(request.Emoji), isCustom: false);
        return await gatherings.SaveAsync(gathering);
    }

    [HttpDelete("{gatheringId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<GatheringDto>> RemoveItem(Guid gatheringId, Guid itemId)
    {
        var gathering = await gatherings.FindAsync(gatheringId);
        var dish = gathering?.DishItems.FirstOrDefault(d => d.Id == itemId);
        if (gathering is null || dish is null) return NotFound("Dish not found.");

        gatherings.RemoveDish(gathering, dish);
        return await gatherings.SaveAsync(gathering);
    }

    private static string? Apply(Gathering gathering, GatheringRequest request)
    {
        var name = Input.Clean(request.Name);
        var hostName = Input.Clean(request.HostName);
        if (name is null) return "Gathering name is required.";
        if (hostName is null) return "Host name is required.";
        if (request.Date == default) return "Date is required.";

        gathering.Name = name;
        gathering.HostName = hostName;
        gathering.Date = Input.ToUtc(request.Date);
        gathering.Location = Input.Clean(request.Location);
        return null;
    }
}
