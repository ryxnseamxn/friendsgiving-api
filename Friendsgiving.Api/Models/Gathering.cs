namespace Friendsgiving.Api.Models;

public class Gathering
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public ICollection<DishItem> DishItems { get; set; } = new List<DishItem>();

    public DishItem AddDish(string name, string? category, string? emoji, bool isCustom)
    {
        // Id is left unset so EF treats the dish as new when it's discovered through this collection.
        var dish = new DishItem
        {
            Name = name,
            Category = DishCategories.Normalize(category),
            Emoji = string.IsNullOrWhiteSpace(emoji) ? DishItem.DefaultEmoji : emoji.Trim(),
            IsCustom = isCustom,
            SortOrder = DishItems.Count == 0 ? 1 : DishItems.Max(d => d.SortOrder) + 1,
        };
        DishItems.Add(dish);
        return dish;
    }
}
