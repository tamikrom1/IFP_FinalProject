using BookingSystem.Models;

namespace BookingSystem.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Resource>> GetAllResourcesAsync() =>
        await _repository.GetActiveResourcesAsync();

    public async Task<Result<IReadOnlyList<TimeSlotDto>>> GetResourceScheduleAsync(
        Guid resourceId, 
        DateTime date, 
        UserRole userRole)
    {
        var resource = await _repository.GetResourceByIdAsync(resourceId);
        if (resource is null || !resource.IsActive)
            return Result<IReadOnlyList<TimeSlotDto>>.Failure("The selected resource cannot be found or is unavailable.");

        var bookings = await _repository.GetBookingsForResourceByDateAsync(resourceId, date);
        var slots = PricingCalculator.GenerateDailySlots(date, resource.PricePerHour, userRole, bookings);

        return Result<IReadOnlyList<TimeSlotDto>>.Success(slots);
    }

    public async Task<Result<Booking>> CreateBookingAsync(CreateBookingRequest request)
    {
        if (request.EndTime <= request.StartTime)
            return Result<Booking>.Failure("The end time must be later than the start time.");

        if (request.StartTime < DateTime.Now)
            return Result<Booking>.Failure("You cannot book a time in the past.");

        var user = await _repository.GetUserByIdAsync(request.UserId);
        if (user is null)
            return Result<Booking>.Failure("User not found");

        var resource = await _repository.GetResourceByIdAsync(request.ResourceId);
        if (resource is null || !resource.IsActive)
            return Result<Booking>.Failure("The resource is not available for booking.");

        var isOccupied = await _repository.HasOverlappingBookingAsync(
            request.ResourceId, 
            request.StartTime, 
            request.EndTime);

        if (isOccupied)
            return Result<Booking>.Failure("Unfortunately, this slot is already taken by another user.");

        var duration = request.EndTime - request.StartTime;
        var totalPrice = PricingCalculator.CalculatePrice(resource.PricePerHour, duration, user.Role);

        var booking = new Booking
        {
            UserId = user.Id,
            ResourceId = resource.Id,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            TotalPrice = totalPrice,
            Status = BookingStatus.Confirmed
        };

        await _repository.AddBookingAsync(booking);
        return Result<Booking>.Success(booking);
    }

    public async Task<Result<bool>> CancelBookingAsync(Guid bookingId, Guid currentUserId, UserRole currentUserRole)
    {
        var booking = await _repository.GetBookingByIdAsync(bookingId);
        if (booking is null)
            return Result<bool>.Failure("Booking not found.");

        var isOwner = booking.UserId == currentUserId;
        var isStaff = currentUserRole is UserRole.Admin or UserRole.Manager;

        if (!isOwner && !isStaff)
            return Result<bool>.Failure("You do not have the rights to cancel this booking.");

        if (booking.Status == BookingStatus.Cancelled)
            return Result<bool>.Failure("The booking has already been cancelled.");

        if (booking.StartTime <= DateTime.Now && !isStaff)
            return Result<bool>.Failure("You cannot cancel a booking that has already begun.");

        booking.Status = BookingStatus.Cancelled;
        await _repository.UpdateBookingAsync(booking);

        return Result<bool>.Success(true);
    }

    public async Task<List<Booking>> GetMyBookingsAsync(Guid userId) =>
        await _repository.GetUserBookingsAsync(userId);
}