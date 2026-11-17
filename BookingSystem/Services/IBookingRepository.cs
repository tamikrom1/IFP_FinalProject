using BookingSystem.Models;

namespace BookingSystem.Services;

public interface IBookingRepository
{
    Task<List<Resource>> GetActiveResourcesAsync();
    Task<Resource?> GetResourceByIdAsync(Guid id);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<Booking?> GetBookingByIdAsync(Guid id);
    Task<List<Booking>> GetBookingsForResourceByDateAsync(Guid resourceId, DateTime date);
    Task<List<Booking>> GetUserBookingsAsync(Guid userId);
    Task<bool> HasOverlappingBookingAsync(Guid resourceId, DateTime start, DateTime end);
    Task AddBookingAsync(Booking booking);
    Task UpdateBookingAsync(Booking booking);
}