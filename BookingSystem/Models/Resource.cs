using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Models;

public class Resource
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Resource name is required")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = "All";

    [Range(1, 500, ErrorMessage = "Must be from 1 to 500")]
    public int Capacity { get; set; } = 1;

    [Range(0, 1000000, ErrorMessage = "Cannot be negative")]
    public decimal PricePerHour { get; set; }

    public string ImageUrl { get; set; } = "/images/default-room.jpg";

    public bool IsActive { get; set; } = true;

    public List<Booking> Bookings { get; set; } = new();
}