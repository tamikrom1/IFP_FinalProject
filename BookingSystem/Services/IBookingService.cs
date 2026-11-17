using BookingSystem.Models;

namespace BookingSystem.Services;

public interface IBookingService
{
    Task<List<Resource>> GetAllResourcesAsync();
    Task<Result<IReadOnlyList<TimeSlotDto>>> GetResourceScheduleAsync(Guid resourceId, DateTime date, UserRole userRole);
    Task<Result<Booking>> CreateBookingAsync(CreateBookingRequest request);
    Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid currentUserId, UserRole currentUserRole);
    Task<List<Booking>> GetMyBookingsAsync(Guid userId);
}