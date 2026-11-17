using BookingSystem.Data;
using BookingSystem.Hubs;
using BookingSystem.Models;
using BookingSystem.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace BookingSystem.State;

public class BookingStateContainer : IAsyncDisposable
{
    private readonly IBookingService _bookingService;
    private readonly IHubContext<BookingHub> _hubContext;
    private readonly NavigationManager _navigationManager;
    private HubConnection? _hubConnection;

    public BookingStateContainer(
        IBookingService bookingService,
        IHubContext<BookingHub> hubContext,
        NavigationManager navigationManager)
    {
        _bookingService = bookingService;
        _hubContext = hubContext;
        _navigationManager = navigationManager;
    }

    public Guid CurrentUserId { get; private set; } = DbInitializer.ClientUserId;
    public UserRole CurrentUserRole { get; private set; } = UserRole.Client;
    public string CurrentUserName { get; private set; } = "Alexey Smirnov (Client)";

    public List<Resource> Resources { get; private set; } = new();
    public Resource? SelectedResource { get; private set; }
    public DateTime SelectedDate { get; private set; } = DateTime.Today;
    public IReadOnlyList<TimeSlotDto> CurrentSlots { get; private set; } = Array.Empty<TimeSlotDto>();
    public List<Booking> MyBookings { get; private set; } = new();

    public bool IsLoading { get; private set; }
    public string? LastNotification { get; private set; }
    public bool IsErrorNotification { get; private set; }

    public event Action? OnStateChanged;

    public async Task InitializeAsync()
    {
        IsLoading = true;
        NotifyStateChanged();

        await EnsureSignalRConnectedAsync();
        Resources = await _bookingService.GetAllResourcesAsync();

        if (Resources.Any() && SelectedResource is null)
        {
            SelectedResource = Resources.First();
        }

        await RefreshScheduleAsync();
        await RefreshMyBookingsAsync();

        IsLoading = false;
        NotifyStateChanged();
    }

    public async Task SwitchDemoUserAsync(UserRole targetRole)
    {
        (CurrentUserId, CurrentUserRole, CurrentUserName) = targetRole switch
        {
            UserRole.Admin => (DbInitializer.AdminUserId, UserRole.Admin, "Administrator (Free)"),
            UserRole.Manager => (DbInitializer.ManagerUserId, UserRole.Manager, "Maria Ivanova (Manager -50%)"),
            _ => (DbInitializer.ClientUserId, UserRole.Client, "Alexey Smirnov (Client)")
        };

        ClearNotification();
        await RefreshScheduleAsync();
        await RefreshMyBookingsAsync();
        NotifyStateChanged();
    }

    public async Task SelectResourceAsync(Resource resource)
    {
        SelectedResource = resource;
        ClearNotification();
        await RefreshScheduleAsync();
        NotifyStateChanged();
    }

    public async Task SelectDateAsync(DateTime date)
    {
        SelectedDate = date.Date;
        ClearNotification();
        await RefreshScheduleAsync();
        NotifyStateChanged();
    }

    public async Task<bool> BookSlotAsync(TimeSlotDto slot)
    {
        if (SelectedResource is null)
            return false;

        var request = new CreateBookingRequest(
            CurrentUserId,
            SelectedResource.Id,
            slot.StartTime,
            slot.EndTime
        );

        var result = await _bookingService.CreateBookingAsync(request);

        return await result.Match<Task<bool>>(
            onSuccess: async booking =>
            {
                SetNotification($"Booking confirmed for {booking.StartTime:HH:mm} - {booking.EndTime:HH:mm} (${booking.TotalPrice}).", false);
                await RefreshScheduleAsync();
                await RefreshMyBookingsAsync();

                await _hubContext.Clients.All.SendAsync("ScheduleUpdated", SelectedResource.Id, SelectedDate);
                NotifyStateChanged();
                return true;
            },
            onFailure: error =>
            {
                SetNotification(error, true);
                NotifyStateChanged();
                return Task.FromResult(false);
            }
        );
    }

    public async Task CancelBookingAsync(Guid bookingId)
    {
        var bookingToCancel = MyBookings.FirstOrDefault(b => b.Id == bookingId);
        var result = await _bookingService.CancelBookingAsync(bookingId, CurrentUserId, CurrentUserRole);

        await result.Match<Task>(
            onSuccess: async _ =>
            {
                SetNotification("Booking has been cancelled.", false);
                await RefreshScheduleAsync();
                await RefreshMyBookingsAsync();

                if (bookingToCancel is not null)
                {
                    await _hubContext.Clients.All.SendAsync("ScheduleUpdated", bookingToCancel.ResourceId, bookingToCancel.StartTime.Date);
                }
                NotifyStateChanged();
            },
            onFailure: error =>
            {
                SetNotification(error, true);
                NotifyStateChanged();
                return Task.CompletedTask;
            }
        );
    }

    public void ClearNotification()
    {
        LastNotification = null;
        IsErrorNotification = false;
    }

    private async Task RefreshScheduleAsync()
    {
        if (SelectedResource is null)
        {
            CurrentSlots = Array.Empty<TimeSlotDto>();
            return;
        }

        var scheduleResult = await _bookingService.GetResourceScheduleAsync(
            SelectedResource.Id,
            SelectedDate,
            CurrentUserRole);

        CurrentSlots = scheduleResult.Match(
            onSuccess: slots => slots,
            onFailure: _ => Array.Empty<TimeSlotDto>()
        );
    }

    private async Task RefreshMyBookingsAsync()
    {
        MyBookings = await _bookingService.GetMyBookingsAsync(CurrentUserId);
    }

    private async Task EnsureSignalRConnectedAsync()
    {
        if (_hubConnection is not null)
            return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/bookinghub"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<Guid, DateTime>("ScheduleUpdated", async (resourceId, updatedDate) =>
        {
            if (SelectedResource?.Id == resourceId && SelectedDate.Date == updatedDate.Date)
            {
                await RefreshScheduleAsync();
                NotifyStateChanged();
            }
        });

        await _hubConnection.StartAsync();
    }

    private void SetNotification(string message, bool isError)
    {
        LastNotification = message;
        IsErrorNotification = isError;
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}