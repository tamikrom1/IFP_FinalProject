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

<img width="8192" height="2299" alt="Resource Booking Architecture-2026-11-17-090218" src="https://github.com/user-attachments/assets/9debaed2-8004-4767-a714-8415af338123" />

---

## Project Structure

```text
IFP_FinalProject/
├── BookingSystem/                  # Main Blazor Server Web Application
│   ├── Components/
│   │   ├── Layout/                 # MainLayout & Navigation
│   │   ├── Pages/                  # Home.razor (Main Dashboard)
│   │   └── Shared/                 # Reusable Components (ResourceCard, TimeSlotGrid)
│   ├── Data/                       # EF Core AppDbContext, BookingRepository, DbInitializer
│   ├── Hubs/                       # SignalR BookingHub
│   ├── Models/                     # Domain Entities, Enums, Immutable Records, Result<T>
│   ├── Services/                   # BookingService, Interfaces, Functional PricingCalculator
│   ├── State/                      # BookingStateContainer
│   └── Program.cs                  # DI Container, SQLite & SignalR Configuration
├── BookingSystem.Tests/            # xUnit Automated Test Suite
│   ├── PricingCalculatorTests.cs   # Unit tests for pure FP pricing & slot generation
│   └── BookingServiceTests.cs      # Unit tests for business rules, overlaps & RBAC
├── docs/                           # Documentation assets & UML diagrams
└── README.md
```

---

## Installation & Setup Guide

### Prerequisites
* .NET 8.0 SDK or newer.

### Running the Application
1. Clone the repository:
   ```bash
   git clone https://github.com/tamikrom1/IFP_FinalProject
   cd IFP_FinalProject
   ```
2. Run the web application (SQLite database `bookingsystem.db` will be created and seeded automatically on first launch):
   ```bash
   dotnet run --project BookingSystem/BookingSystem.csproj
   ```
3. Open your browser and navigate to the local URL displayed in the terminal (e.g., `http://localhost:5123`).

### Running Automated Tests
Execute all unit tests from the root directory:
```bash
dotnet test
```

---

## User Guide & Demo Walkthrough

1. **Select a Resource:** Click on any room or workspace card in Section 1 to load its availability.
2. **Choose Date & Time Slot:** Pick a target date in Section 2. Available slots are highlighted in green with calculated prices. Click a slot to book it instantly.
3. **Test Real-Time Sync (SignalR):** Open the application in two side-by-side browser windows. Booking or cancelling a slot in one window immediately updates the slot availability in the second window without refreshing.
4. **Test Role-Based Pricing & Permissions (RBAC):** Use the **Active Role** selector in the top-right header to switch between `Client` (100% rate), `Manager` (50% discount), and `Admin` (100% discount) to observe real-time price recalculations.
5. **Manage Bookings:** View your active and cancelled reservations in Section 3 and click **Cancel** to release a booked slot back to the public schedule.
