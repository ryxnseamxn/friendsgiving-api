namespace Friendsgiving.Api.Models;

/// <summary>A guest's answer to "any allergies?". Null <see cref="Details"/> means they have none.</summary>
public class Allergy
{
    public Guid Id { get; set; }
    public Guid GatheringId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? Details { get; set; }
    public virtual Gathering Gathering { get; set; } = null!;
}
