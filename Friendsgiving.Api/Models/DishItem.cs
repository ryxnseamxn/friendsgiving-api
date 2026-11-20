namespace Friendsgiving.Api.Models;

public class DishItem
{
    public const string DefaultEmoji = "🍽️";

    public Guid Id { get; set; }
    public Guid GatheringId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = DishCategories.Other;
    public string Emoji { get; set; } = DefaultEmoji;
    public bool IsCustom { get; set; }
    public int SortOrder { get; set; }
    public virtual Gathering Gathering { get; set; } = null!;
    public virtual Claim? Claim { get; set; }

    public void ClaimFor(string guestName, string? note) =>
        Claim = new Claim { GuestName = guestName, Note = note, ClaimedAt = DateTime.UtcNow };
}
