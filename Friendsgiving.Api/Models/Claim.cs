namespace Friendsgiving.Api.Models;

public class Claim
{
    public Guid Id { get; set; }
    public Guid DishItemId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime ClaimedAt { get; set; }
    public virtual DishItem DishItem { get; set; } = null!;
}