using BookingSystem.Models;
using BookingSystem.Services;

namespace BookingSystem.Tests;

public class PricingCalculatorTests
{
    [Theory]
    [InlineData(100, 1, UserRole.Client, 100)]     // Standard rate: 100 * 1 = 100
    [InlineData(100, 2, UserRole.Manager, 100)]    // Manager 50% discount: 200 * 0.5 = 100
    [InlineData(100, 3, UserRole.Admin, 0)]        // Admin free access: 0
    [InlineData(100, 4, UserRole.Client, 360)]     // Bulk duration (>= 4h) 10% discount: 400 * 0.9 = 360
    [InlineData(100, 4, UserRole.Manager, 180)]    // Combined Manager (50%) + Bulk (10%): 400 * 0.5 * 0.9 = 180
    public void CalculatePrice_ReturnsExpectedDiscountedAmount(
        decimal hourlyRate,
        int hours,
        UserRole role,
        decimal expectedPrice)
    {
        // Arrange
        var duration = TimeSpan.FromHours(hours);

        // Act
        var actualPrice = PricingCalculator.CalculatePrice(hourlyRate, duration, role);

        // Assert
        Assert.Equal(expectedPrice, actualPrice);
    }

    [Fact]
    public void CalculatePrice_NegativeRateOrZeroDuration_ReturnsZero()
    {
        Assert.Equal(0m, PricingCalculator.CalculatePrice(-50m, TimeSpan.FromHours(2), UserRole.Client));
        Assert.Equal(0m, PricingCalculator.CalculatePrice(100m, TimeSpan.Zero, UserRole.Client));
    }

    [Fact]
    public void GenerateDailySlots_MarksOverlappingSlotsAsUnavailable()
    {
        // Arrange
        var tomorrow = DateTime.Today.AddDays(1);
        var existingBookings = new List<Booking>
        {
            new()
            {
                StartTime = tomorrow.AddHours(10),
                EndTime = tomorrow.AddHours(12),
                Status = BookingStatus.Confirmed
            },
            new()
            {
                // Cancelled booking should NOT block the slot
                StartTime = tomorrow.AddHours(14),
                EndTime = tomorrow.AddHours(15),
                Status = BookingStatus.Cancelled
            }
        };

        // Act
        var slots = PricingCalculator.GenerateDailySlots(
            tomorrow,
            hourlyRate: 50m,
            UserRole.Client,
            existingBookings,
            startHour: 9,
            endHour: 16);

        // Assert
        Assert.Equal(7, slots.Count);
        Assert.True(slots.First(s => s.StartTime.Hour == 9).IsAvailable);
        Assert.False(slots.First(s => s.StartTime.Hour == 10).IsAvailable);
        Assert.False(slots.First(s => s.StartTime.Hour == 11).IsAvailable);
        Assert.True(slots.First(s => s.StartTime.Hour == 12).IsAvailable);
        Assert.True(slots.First(s => s.StartTime.Hour == 14).IsAvailable);
    }
}