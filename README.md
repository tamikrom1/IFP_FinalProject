# Real-Time Resource Booking System

A cross-platform web application for booking workspaces, meeting rooms, and media studios in real time. Built with **C#**, **.NET 8**, **Blazor Server**, **Entity Framework Core**, and **SignalR**, following Clean Architecture and Functional Programming (FP) principles.

## Team Members
* **Tamerlan Iskakov** — Architecture, Backend Services, Functional Pricing Engine
* **Madina Kairbayeva** — SignalR Real-Time Sync, Unit Testing, UI Components

---

## Key Features & Criteria Coverage

* **Clean Architecture & SOLID:** Strict separation of concerns across `Models`, `Data` (Repository pattern), `Services`, `State`, `Hubs`, and `Components`.
* **Functional Programming (FP) Principles:**
  * Stateless, pure functions in `PricingCalculator` with deterministic outputs and zero side effects.
  * Pattern matching (`switch` expressions) for role-based discount calculation.
  * Immutable `record` types (`CreateBookingRequest`, `TimeSlotDto`, `Result<T>`) for safe data transfer and railway-oriented error handling without expensive exceptions.
* **Centralized State Management:** `BookingStateContainer` manages application state, active user sessions, and triggers reactive UI updates via events (`OnStateChanged`).
* **Real-Time Synchronization (SignalR):** `BookingHub` broadcasts slot updates across all active browser sessions immediately when a booking is created or cancelled, preventing double-booking conflicts.
* **Role-Based Access Control (RBAC):** Built-in demo role switcher supporting `Client` (standard rate), `Manager` (50% discount), and `Admin` (free access + override cancellation privileges).
* **Reusable & Responsive Blazor Components:** Modular components (`ResourceCard`, `TimeSlotGrid`) utilizing `[Parameter]` and `EventCallback`, styled with responsive Bootstrap 5 grids for desktop, tablet, and mobile viewports.
* **Automated Testing:** Comprehensive `xUnit` and `Moq` test suite covering pure pricing logic, slot generation, overlap validation, and RBAC rules.

---

## System Architecture (UML Class Diagram)

```mermaid
classDiagram
    class User {
        +Guid Id
        +string FullName
        +string Email
        +UserRole Role
        +List~Booking~ Bookings
    }

    class Resource {
        +Guid Id
        +string Name
        +string Category
        +int Capacity
        +decimal PricePerHour
        +bool IsActive
    }

    class Booking {
        +Guid Id
        +Guid UserId
        +Guid ResourceId
        +DateTime StartTime
        +DateTime EndTime
        +decimal TotalPrice
        +BookingStatus Status
        +CalculateDuration() TimeSpan
        +IsOverlapping(DateTime start, DateTime end) bool
    }

    class PricingCalculator {
        <<Functional Logic Pure>>
        +CalculatePrice(decimal hourlyRate, TimeSpan duration, UserRole role)$ decimal
        +GenerateDailySlots(DateTime date, decimal hourlyRate, UserRole role, IEnumerable~Booking~ existingBookings)$ IReadOnlyList~TimeSlotDto~
    }

    class IBookingService {
        <<interface>>
        +GetAllResourcesAsync() Task~List~Resource~~
        +GetResourceScheduleAsync(Guid resourceId, DateTime date, UserRole userRole) Task~Result~IReadOnlyList~TimeSlotDto~~~
        +CreateBookingAsync(CreateBookingRequest request) Task~Result~Booking~~
        +CancelBookingAsync(Guid bookingId, Guid currentUserId, UserRole currentUserRole) Task~Result~bool~~
    }

    class BookingService {
        -IBookingRepository _repository
    }

    class IBookingRepository {
        <<interface>>
    }

    class BookingRepository {
        -AppDbContext _context
    }

    class BookingStateContainer {
        +List~Resource~ Resources
        +Resource SelectedResource
        +DateTime SelectedDate
        +IReadOnlyList~TimeSlotDto~ CurrentSlots
        +event Action OnStateChanged
        +BookSlotAsync(TimeSlotDto slot) Task~bool~
        +CancelBookingAsync(Guid bookingId) Task
    }

    class BookingHub {
        +NotifyScheduleChanged(Guid resourceId, DateTime date) Task
    }

    User "1" --> "*" Booking : creates
    Resource "1" --> "*" Booking : booked in
    BookingService ..|> IBookingService : implements
    BookingRepository ..|> IBookingRepository : implements
    BookingService --> IBookingRepository : depends on
    BookingService --> PricingCalculator : uses
    BookingStateContainer --> IBookingService : calls
    BookingStateContainer --> BookingHub : listens to