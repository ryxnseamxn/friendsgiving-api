namespace Friendsgiving.Api.Models;

public static class DishCategories
{
    public const string Other = "other";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string> { "main", "side", "dessert", "drink", "appetizer", Other };

    public static string Normalize(string? category)
    {
        var value = category?.Trim().ToLowerInvariant();
        return value is not null && All.Contains(value) ? value : Other;
    }
}
