using Coforge_Task.Models;
using Microsoft.EntityFrameworkCore;

namespace Coforge_Task.Data;

/// <summary>
/// Entity Framework Core context for gift card purchase management.
/// Configuration assumes SQL Server is being used as the database provider.
/// </summary>
public class GiftCardContext : DbContext
{
    public GiftCardContext(DbContextOptions<GiftCardContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OrderId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CustomerId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.GiftCardType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.GiftCardCode)
                .HasMaxLength(100);

            entity.Property(e => e.Amount)
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.OrderId)
                .IsUnique();

            entity.HasIndex(e => e.CustomerId);
        });
    }
}
