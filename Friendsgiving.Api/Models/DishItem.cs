namespace Friendsgiving.Api.Models;

public class DishItem
{
    public Guid Id { get; set; }
    public Guid GatheringId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public int SortOrder { get; set; }
    public virtual Gathering Gathering { get; set; } = null!;
    public virtual Claim? Claim { get; set; }
}