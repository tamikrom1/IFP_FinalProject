namespace BookingSystem.Models;

public sealed record CreateBookingRequest(
    Guid UserId,
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
);

public sealed record TimeSlotDto(
    DateTime StartTime,
    DateTime EndTime,
    bool IsAvailable,
    decimal CalculatedPrice
);