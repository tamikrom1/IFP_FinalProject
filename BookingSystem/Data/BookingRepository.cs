using BookingSystem.Models;
using BookingSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Data;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Resource>> GetActiveResourcesAsync() =>
        await _context.Resources
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Category)
            .ThenBy(r => r.Name)
            .ToListAsync();

    public async Task<Resource?> GetResourceByIdAsync(Guid id) =>
        await _context.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<User?> GetUserByIdAsync(Guid id) =>
        await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Booking?> GetBookingByIdAsync(Guid id) =>
        await _context.Bookings
            .Include(b => b.Resource)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<Booking>> GetBookingsForResourceByDateAsync(Guid resourceId, DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.ResourceId == resourceId &&
                        b.Status != BookingStatus.Cancelled &&
                        b.StartTime < dayEnd &&
                        b.EndTime > dayStart)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetUserBookingsAsync(Guid userId) =>
        await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Resource)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.StartTime)
            .ToListAsync();

    public async Task<bool> HasOverlappingBookingAsync(Guid resourceId, DateTime start, DateTime end) =>
        await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.ResourceId == resourceId &&
                           b.Status != BookingStatus.Cancelled &&
                           b.StartTime < end &&
                           b.EndTime > start);

    public async Task AddBookingAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateBookingAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }
}