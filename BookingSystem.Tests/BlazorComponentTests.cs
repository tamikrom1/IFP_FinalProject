using BookingSystem.Components.Shared;
using BookingSystem.Models;
using Bunit;

namespace BookingSystem.Tests;

public class BlazorComponentTests : TestContext
{
    [Fact]
    public void ResourceCard_RendersDetailsAndInvokesCallbackOnClick()
    {
        // Arrange
        var resource = new Resource
        {
            Name = "Conference Room Alpha",
            Category = "Meeting Rooms",
            Capacity = 12,
            PricePerHour = 50m,
            Description = "4K Display included"
        };

        Resource? selectedResource = null;

        var cut = RenderComponent<ResourceCard>(parameters => parameters
            .Add(p => p.Resource, resource)
            .Add(p => p.IsSelected, false)
            .Add(p => p.OnSelect, r => selectedResource = r));

        // Act
        cut.Find(".card").Click();

        // Assert
        Assert.Contains("Conference Room Alpha", cut.Markup);
        Assert.Contains("$50 / hr", cut.Markup);
        Assert.Equal(resource.Id, selectedResource?.Id);
    }

    [Fact]
    public void TimeSlotGrid_DisablesBookedSlotsAndBooksAvailableOnes()
    {
        // Arrange
        var start = DateTime.Today.AddDays(1).AddHours(10);
        var slots = new List<TimeSlotDto>
        {
            new(start, start.AddHours(1), IsAvailable: true, CalculatedPrice: 50m),
            new(start.AddHours(1), start.AddHours(2), IsAvailable: false, CalculatedPrice: 50m)
        };

        TimeSlotDto? bookedSlot = null;

        var cut = RenderComponent<TimeSlotGrid>(parameters => parameters
            .Add(p => p.Slots, slots)
            .Add(p => p.IsBusy, false)
            .Add(p => p.OnBookSlot, s => bookedSlot = s));

        var buttons = cut.FindAll("button");

        // Act & Assert
        Assert.Equal(2, buttons.Count);
        Assert.False(buttons[0].HasAttribute("disabled"));
        Assert.True(buttons[1].HasAttribute("disabled"));

        buttons[0].Click();
        Assert.Equal(start, bookedSlot?.StartTime);
    }
}