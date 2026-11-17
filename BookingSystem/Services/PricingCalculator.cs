using BookingSystem.Models;

namespace BookingSystem.Services;

public static class PricingCalculator
{
    public static decimal CalculatePrice(decimal hourlyRate, TimeSpan duration, UserRole role)
    {
        if (hourlyRate < 0 || duration <= TimeSpan.Zero)
            return 0m;

        var hours = (decimal)duration.TotalHours;
        var basePrice = hourlyRate * hours;

        var roleMultiplier = role switch
        {
            UserRole.Admin => 0.0m,
            UserRole.Manager => 0.5m,
            UserRole.Client => 1.0m,
            _ => 1.0m
        };

        var durationMultiplier = hours >= 4.0m ? 0.9m : 1.0m;

        return Math.Round(basePrice * roleMultiplier * durationMultiplier, 2);
    }

    public static IReadOnlyList<TimeSlotDto> GenerateDailySlots(
        DateTime date,
        decimal hourlyRate,
        UserRole role,
        IEnumerable<Booking> existingBookings,
        int startHour = 9,
        int endHour = 21)
    {
        var dayStart = date.Date;

        return Enumerable.Range(startHour, endHour - startHour)
            .Select(hour =>
            {
                var slotStart = dayStart.AddHours(hour);
                var slotEnd = slotStart.AddHours(1);

                var isAvailable = slotStart > DateTime.Now &&
                                  !existingBookings.Any(b => b.IsOverlapping(slotStart, slotEnd));

                var price = CalculatePrice(hourlyRate, TimeSpan.FromHours(1), role);

                return new TimeSlotDto(slotStart, slotEnd, isAvailable, price);
            })
            .ToList()
            .AsReadOnly();
    }
}