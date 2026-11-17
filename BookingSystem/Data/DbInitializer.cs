using BookingSystem.Models;

namespace BookingSystem.Data;

public static class DbInitializer
{
    public static readonly Guid ClientUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ManagerUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!context.Users.Any())
        {
            var users = new List<User>
            {
                new()
                {
                    Id = ClientUserId,
                    FullName = "Alexey Smirnov (Client)",
                    Email = "client@booking.local",
                    Role = UserRole.Client
                },
                new()
                {
                    Id = ManagerUserId,
                    FullName = "Maria Ivanova (Manager - 50% discount)",
                    Email = "manager@booking.local",
                    Role = UserRole.Manager
                },
                new()
                {
                    Id = AdminUserId,
                    FullName = "Administrator (Free)",
                    Email = "admin@booking.local",
                    Role = UserRole.Admin
                }
            };

            await context.Users.AddRangeAsync(users);
        }

        if (!context.Resources.Any())
        {
            var resources = new List<Resource>
            {
                new()
                {
                    Name = "Meeting Room",
                    Description = "Spacious conference room with a 4K screen, flip chart, and video conferencing system.",
                    Category = "Meeting rooms",
                    Capacity = 12,
                    PricePerHour = 5000m
                },
                new()
                {
                    Name = "Workplace #1",
                    Description = "A quiet, soundproofed booth for calls and deep work.",
                    Category = "Coworking",
                    Capacity = 1,
                    PricePerHour = 1500m
                },
                new()
                {
                    Name = "Lecture hall",
                    Description = "A space for workshops, meetups, and presentations, equipped with a projector and microphones.",
                    Category = "Halls",
                    Capacity = 50,
                    PricePerHour = 15000m
                },
                new()
                {
                    Name = "Podcast studio",
                    Description = "Professional microphones, a mixing console, and studio lighting.",
                    Category = "Media",
                    Capacity = 4,
                    PricePerHour = 8000m
                }
            };

            await context.Resources.AddRangeAsync(resources);
        }

        await context.SaveChangesAsync();
    }
}