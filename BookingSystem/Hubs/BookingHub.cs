using Microsoft.AspNetCore.SignalR;

namespace BookingSystem.Hubs;

public class BookingHub : Hub
{
    public async Task NotifyScheduleChanged(Guid resourceId, DateTime date)
    {
        await Clients.All.SendAsync("ScheduleUpdated", resourceId, date.Date);
    }
}