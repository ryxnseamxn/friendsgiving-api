using Microsoft.EntityFrameworkCore;
using Friendsgiving.Api.Models;

namespace Friendsgiving.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Gathering> Gatherings { get; set; } = null!;
    public DbSet<DishItem> DishItems { get; set; } = null!;
    public DbSet<Claim> Claims { get; set; } = null!;
    public DbSet<Allergy> Allergies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Gathering => many DishItems (one-to-many)
        modelBuilder.Entity<Gathering>()
            .HasMany(g => g.DishItems)
            .WithOne(d => d.Gathering)
            .HasForeignKey(d => d.GatheringId)
            .OnDelete(DeleteBehavior.Cascade);

        // Claim => one DishItem (one-to-one, Claim is dependent side)
        modelBuilder.Entity<Claim>()
            .HasOne(c => c.DishItem)
            .WithOne(d => d.Claim)
            .HasForeignKey<Claim>(c => c.DishItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique claim per dish item
        modelBuilder.Entity<Claim>()
            .HasIndex(c => c.DishItemId)
            .IsUnique();

        // One allergy answer per guest per gathering
        modelBuilder.Entity<Allergy>()
            .HasOne(a => a.Gathering)
            .WithMany(g => g.Allergies)
            .HasForeignKey(a => a.GatheringId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Allergy>()
            .HasIndex(a => new { a.GatheringId, a.GuestName })
            .IsUnique();
    }
}