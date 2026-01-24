using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SQLServer;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Container> Containers => Set<Container>();

    public DbSet<ContainerItem> ContainerItems => Set<ContainerItem>();

    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Container>(entity =>
        {
            entity.HasKey(e => e.ContainerId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasMany(e => e.InventoryItems)
                  .WithOne()
                  .HasForeignKey(e => e.ContainerId);
        });

        modelBuilder.Entity<ContainerItem>(entity =>
        {
            entity.HasKey(e => e.ContainerItemId);

            entity.HasOne<Item>()
                  .WithMany()
                  .HasForeignKey(e => e.ItemId);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });
    }
}
