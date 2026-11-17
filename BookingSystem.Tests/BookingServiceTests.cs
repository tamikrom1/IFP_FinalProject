using BookingSystem.Models;
using BookingSystem.Services;
using Moq;

namespace BookingSystem.Tests;

public class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _repositoryMock;
    private readonly BookingService _service;

    public BookingServiceTests()
    {
        _repositoryMock = new Mock<IBookingRepository>();
        _service = new BookingService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_ValidRequest_ReturnsSuccessAndSavesBooking()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var start = DateTime.Now.AddHours(2);
        var end = start.AddHours(2);

        _repositoryMock.Setup(r => r.GetUserByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Role = UserRole.Client });

        _repositoryMock.Setup(r => r.GetResourceByIdAsync(resourceId))
            .ReturnsAsync(new Resource { Id = resourceId, PricePerHour = 100m, IsActive = true });

        _repositoryMock.Setup(r => r.HasOverlappingBookingAsync(resourceId, start, end))
            .ReturnsAsync(false);

        var request = new CreateBookingRequest(userId, resourceId, start, end);

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(200m, result.Value!.TotalPrice);
        _repositoryMock.Verify(r => r.AddBookingAsync(It.IsAny<Booking>()), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_SlotAlreadyOccupied_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var start = DateTime.Now.AddHours(2);
        var end = start.AddHours(1);

        _repositoryMock.Setup(r => r.GetUserByIdAsync(userId))
            .ReturnsAsync(new User { Id = userId, Role = UserRole.Client });

        _repositoryMock.Setup(r => r.GetResourceByIdAsync(resourceId))
            .ReturnsAsync(new Resource { Id = resourceId, PricePerHour = 100m, IsActive = true });

        // Simulate concurrent booking conflict
        _repositoryMock.Setup(r => r.HasOverlappingBookingAsync(resourceId, start, end))
            .ReturnsAsync(true);

        var request = new CreateBookingRequest(userId, resourceId, start, end);

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        _repositoryMock.Verify(r => r.AddBookingAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CreateBookingAsync_StartTimeInPast_ReturnsFailure()
    {
        // Arrange
        var request = new CreateBookingRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Now.AddHours(-2),
            DateTime.Now.AddHours(-1));

        // Act
        var result = await _service.CreateBookingAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CancelBookingAsync_DifferentClientUser_ReturnsFailure()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetBookingByIdAsync(bookingId))
            .ReturnsAsync(new Booking
            {
                Id = bookingId,
                UserId = ownerId,
                StartTime = DateTime.Now.AddHours(3),
                Status = BookingStatus.Confirmed
            });

        // Act
        var result = await _service.CancelBookingAsync(bookingId, otherClientId, UserRole.Client);

        // Assert
        Assert.False(result.IsSuccess);
        _repositoryMock.Verify(r => r.UpdateBookingAsync(It.IsAny<Booking>()), Times.Never);
    }

    [Fact]
    public async Task CancelBookingAsync_AdminUser_CanCancelAnyBooking()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetBookingByIdAsync(bookingId))
            .ReturnsAsync(new Booking
            {
                Id = bookingId,
                UserId = ownerId,
                StartTime = DateTime.Now.AddHours(3),
                Status = BookingStatus.Confirmed
            });

        // Act
        var result = await _service.CancelBookingAsync(bookingId, adminId, UserRole.Admin);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.UpdateBookingAsync(It.Is<Booking>(b => b.Status == BookingStatus.Cancelled)), Times.Once);
    }
}