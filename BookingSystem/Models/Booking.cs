using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public Guid ResourceId { get; set; }
    public Resource? Resource { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TimeSpan CalculateDuration() => 
        EndTime > StartTime ? EndTime - StartTime : TimeSpan.Zero;

    public bool IsOverlapping(DateTime otherStart, DateTime otherEnd) =>
        Status != BookingStatus.Cancelled &&
        StartTime < otherEnd && 
        EndTime > otherStart;
}