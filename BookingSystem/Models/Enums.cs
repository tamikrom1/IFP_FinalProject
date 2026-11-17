namespace BookingSystem.Models;

public enum UserRole
{
    Guest = 0,
    Client = 1,
    Manager = 2,
    Admin = 3
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}