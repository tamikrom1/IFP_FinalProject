using BookingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.Property(r => r.PricePerHour).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.TotalPrice).HasPrecision(18, 2);

            entity.HasIndex(b => new { b.ResourceId, b.StartTime, b.EndTime });

            entity.HasOne(b => b.User)
                  .WithMany(u => u.Bookings)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Resource)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(b => b.ResourceId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}