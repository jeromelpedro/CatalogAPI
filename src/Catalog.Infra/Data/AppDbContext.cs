using Catalog.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infra.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<UserGame> UserGames => Set<UserGame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Name).IsRequired().HasMaxLength(200);
            e.Property(g => g.Genre).IsRequired().HasMaxLength(100);
            e.Property(g => g.Price).HasColumnType("decimal(18,2)");
            e.Property(g => g.PromotionalPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<UserGame>(e =>
        {
            e.HasKey(ug => ug.Id);
            e.HasIndex(ug => new { ug.UserId, ug.GameId }).IsUnique();
        });
    }
}
