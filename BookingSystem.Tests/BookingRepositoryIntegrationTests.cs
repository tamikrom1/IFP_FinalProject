using BookingSystem.Data;
using BookingSystem.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Tests;

public class BookingRepositoryIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly BookingRepository _repository;

    public BookingRepositoryIntegrationTests()
    {
        // Use in-memory SQLite database for fast, isolated integration testing
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new BookingRepository(_context);
    }

    [Fact]
    public async Task AddBookingAndCheckOverlap_PersistsToDatabaseCorrectly()
    {
        // Arrange
        var user = new User { FullName = "Integration User", Email = "test@db.local", Role = UserRole.Client };
        var resource = new Resource { Name = "Test Room", Category = "Rooms", PricePerHour = 100m, IsActive = true };

        await _context.Users.AddAsync(user);
        await _context.Resources.AddAsync(resource);
        await _context.SaveChangesAsync();

        var start = DateTime.Today.AddDays(1).AddHours(10);
        var end = start.AddHours(2);

        var booking = new Booking
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartTime = start,
            EndTime = end,
            TotalPrice = 200m,
            Status = BookingStatus.Confirmed
        };

        // Act
        await _repository.AddBookingAsync(booking);
        var hasOverlap = await _repository.HasOverlappingBookingAsync(resource.Id, start.AddHours(1), end.AddHours(1));
        var userBookings = await _repository.GetUserBookingsAsync(user.Id);

        // Assert
        Assert.True(hasOverlap);
        Assert.Single(userBookings);
        Assert.Equal("Test Room", userBookings.First().Resource?.Name);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}