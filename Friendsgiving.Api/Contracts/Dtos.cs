using Friendsgiving.Api.Models;

namespace Friendsgiving.Api.Contracts;

// Responses. Every mutation returns the full gathering, and the hub broadcasts the same shape,
// so clients replace their state wholesale instead of patching it.

public record GatheringDto(
    Guid Id,
    string Name,
    DateTime Date,
    string HostName,
    string? Location,
    IReadOnlyList<ItemDto> Items,
    IReadOnlyList<AllergyDto> Allergies);

public record ItemDto(
    Guid Id,
    string Name,
    string Category,
    string Emoji,
    bool IsCustom,
    int SortOrder,
    ClaimDto? Claim);

public record ClaimDto(string GuestName, string? Note, DateTime ClaimedAt);

public record AllergyDto(string GuestName, string? Details);

public record GatheringSummaryDto(
    Guid Id,
    string Name,
    DateTime Date,
    string HostName,
    string? Location,
    int ItemCount,
    int ClaimedCount);

// Requests

public record GatheringRequest(string Name, DateTime Date, string HostName, string? Location);

public record AddItemRequest(string Name, string? Category, string? Emoji, string? ClaimedBy);

public record ClaimRequest(string GuestName, string? Note);

public record AllergyRequest(string GuestName, string? Details);

public static class Mapping
{
    public static GatheringDto ToDto(this Gathering gathering) => new(
        gathering.Id,
        gathering.Name,
        gathering.Date,
        gathering.HostName,
        gathering.Location,
        gathering.DishItems
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Name)
            .Select(d => d.ToDto())
            .ToList(),
        gathering.Allergies
            .OrderBy(a => a.GuestName)
            .Select(a => new AllergyDto(a.GuestName, a.Details))
            .ToList());

    public static ItemDto ToDto(this DishItem dish) => new(
        dish.Id,
        dish.Name,
        dish.Category,
        dish.Emoji,
        dish.IsCustom,
        dish.SortOrder,
        dish.Claim is { } claim ? new ClaimDto(claim.GuestName, claim.Note, claim.ClaimedAt) : null);
}
