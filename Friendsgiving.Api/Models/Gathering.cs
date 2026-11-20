namespace Friendsgiving.Api.Models;

public class Gathering
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public ICollection<DishItem> DishItems { get; set; } = new List<DishItem>();
}