# Conference Rooms API

REST API for managing conference rooms, services, bookings, availability, and rental costs.

The project is built with **ASP.NET Core**, **Entity Framework Core**, **PostgreSQL** and follows **Clean Architecture** principles with a strong separation between Domain, Application, Infrastructure, and API layers.

---

## Features

* Create, update and delete conference rooms
* Create, update and delete additional services
* Search for available rooms
* Book one or multiple rooms
* Assign different services to each booked room
* Calculate booking costs automatically
* Modify existing bookings
* Cancel bookings
* Preserve historical room and service prices using booking snapshots
* PostgreSQL persistence
* EF Core migrations
* Initial database seeding
* Domain-driven business rules
* Dependency Injection
* Asynchronous database operations
* CancellationToken support

---

## Tech Stack

| Technology            | Purpose                        |
| --------------------- | ------------------------------ |
| C#                    | Main programming language      |
| ASP.NET Core          | REST API                       |
| .NET                  | Application platform           |
| Entity Framework Core | ORM                            |
| PostgreSQL            | Database                       |
| Npgsql                | PostgreSQL EF Core provider    |
| JSONB                 | Booking room/service snapshots |
| Dependency Injection  | Application composition        |
| Clean Architecture    | Project architecture           |

---

## Architecture

The solution is divided into four projects:

```text
ConferenceRooms
│
├── ConferenceRooms.Domain
│
├── ConferenceRooms.Application
│
├── ConferenceRooms.Infrastructure
│
└── ConferenceRooms.Api
```

### Dependency Flow

```text
                 ┌─────────────────────┐
                 │   ConferenceRooms   │
                 │         API         │
                 └──────────┬──────────┘
                            │
                 ┌──────────▼──────────┐
                 │    Application      │
                 └──────────┬──────────┘
                            │
                 ┌──────────▼──────────┐
                 │       Domain        │
                 └─────────────────────┘
                            ▲
                            │
                 ┌──────────┴──────────┐
                 │   Infrastructure    │
                 └─────────────────────┘
```

### Domain

Contains core business logic and entities.

```text
Domain
├── Entities
│   ├── Room
│   ├── Service
│   └── Booking
│
├── Models
│   ├── BookingRoom
│   └── BookingService
│
├── Enums
│   └── BookingStatus
│
└── Services
    └── BookingPriceCalculator
```

The Domain layer has no dependency on EF Core, PostgreSQL, ASP.NET Core, or other infrastructure concerns.

---

### Application

Contains use cases and application orchestration.

```text
Application
├── Abstractions
│   └── Persistence
│       ├── IRoomRepository
│       ├── IServiceRepository
│       └── IBookingRepository
│
├── Rooms
│   ├── Commands
│   │   ├── CreateRoom
│   │   ├── UpdateRoom
│   │   └── DeleteRoom
│   │
│   └── Queries
│       └── GetAvailableRooms
│
├── Services
│   └── Commands
│       ├── CreateService
│       ├── UpdateService
│       └── DeleteService
│
└── Bookings
    ├── Commands
    │   ├── CreateBooking
    │   ├── UpdateBooking
    │   └── CancelBooking
    │
    └── Queries
        └── GetBookingById
```

The Application layer depends only on abstractions rather than concrete database implementations.

---

### Infrastructure

Responsible for persistence and external infrastructure.

```text
Infrastructure
├── Persistence
│   ├── ApplicationDbContext
│   │
│   ├── Configurations
│   │   ├── RoomConfiguration
│   │   ├── ServiceConfiguration
│   │   └── BookingConfiguration
│   │
│   ├── Repositories
│   │   ├── RoomRepository
│   │   ├── ServiceRepository
│   │   └── BookingRepository
│   │
│   └── Seed
│       └── DatabaseSeeder
│
└── DependencyInjection
```

---

### API

Contains HTTP endpoints, controllers, middleware and API configuration.

```text
Api
├── Controllers
├── Middleware
├── Program.cs
└── appsettings.json
```

---

# Domain Model

## Room

A conference room contains:

* `Id`
* `Name`
* `Capacity`
* `BaseHourlyRate`
* `IsActive`

Initial rooms:

| Room   | Capacity | Hourly Rate |
| ------ | -------: | ----------: |
| Room A |       50 |    2000 UAH |
| Room B |      100 |    3500 UAH |
| Room C |       30 |    1500 UAH |

---

## Service

Additional services available for rooms:

| Service   |   Price |
| --------- | ------: |
| Projector | 500 UAH |
| Wi-Fi     | 300 UAH |
| Sound     | 700 UAH |

Services have their own lifecycle and can be activated/deactivated independently from rooms.

---

## Booking

A booking contains:

* `Id`
* `StartAt`
* `EndAt`
* `Status`
* `TotalPrice`
* booked rooms
* selected services

A booking can contain multiple rooms.

Each room can have its own set of selected services.

Example:

```text
Booking
│
├── Room A
│   ├── Projector
│   └── Wi-Fi
│
└── Room B
    └── Sound
```

---

# Booking Snapshots

Bookings store selected room and service information as a **snapshot**.

The snapshot contains:

```text
Room
├── RoomId
├── Name
├── Capacity
├── HourlyRate
├── RentalPrice
├── ServicesPrice
│
└── Services
    ├── ServiceId
    ├── Name
    └── Price
```

The snapshot is stored in PostgreSQL using `JSONB`.

### Why snapshots?

Suppose a room currently costs:

```text
2000 UAH/hour
```

A customer creates a booking.

Later the administrator changes the room price to:

```text
2500 UAH/hour
```

The existing booking must still contain the original:

```text
2000 UAH/hour
```

The same applies to service names and prices.

Therefore, the booking represents the **historical state at the moment of booking**, rather than dynamically referencing the current room/service data.

---

# Pricing

The booking price is calculated by the Domain Service:

```text
IBookingPriceCalculator
        │
        ▼
BookingPriceCalculator
```

Pricing rules:

| Time        | Price Adjustment |
| ----------- | ---------------: |
| 06:00–09:00 |             -10% |
| 09:00–18:00 |         Standard |
| 12:00–14:00 |             +15% |
| 18:00–23:00 |             -20% |

The peak period has priority over the standard daytime rate.

Services are charged as additional booking costs.

```text
Total Booking Price
        =
Room Rental Prices
        +
Selected Services
```

---

# Availability

A room is available when:

1. The room is active.
2. Its capacity is sufficient.
3. There is no confirmed booking overlapping the requested period.

Booking overlap is determined using:

```text
existing.StartAt < requested.EndAt
AND
existing.EndAt > requested.StartAt
```

For example:

```text
Existing booking:
10:00 ───────── 12:00

Requested:
11:00 ───────── 13:00

Result: CONFLICT
```

While:

```text
Existing booking:
10:00 ───────── 12:00

Requested:
12:00 ───────── 14:00

Result: AVAILABLE
```

---

# Booking Lifecycle

Bookings use the following statuses:

```text
Confirmed
Cancelled
Completed
```

Allowed operations:

```text
Confirmed
   │
   ├── Update
   ├── Cancel
   └── Complete
```

Cancelled and completed bookings cannot be modified.

---

# Database

The application uses PostgreSQL.

Main tables:

```text
Rooms
Services
Bookings
```

Room/service relationships are independent from booking snapshots.

Bookings store their historical room/service information inside a JSONB column.

Conceptually:

```text
Rooms ───────────────┐
                     │
Services ────────────┤
                     │
                     ▼
                 Booking
                     │
                     ▼
               JSONB Snapshot
```

---

# Configuration

Add the PostgreSQL connection string to:

```text
ConferenceRooms.Api/appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ConferenceRooms;Username=postgres;Password=your_password"
  }
}
```

For production environments, credentials should be provided through environment variables or a dedicated secret-management solution rather than committed to source control.

---

# Getting Started

## Requirements

Install:

* .NET SDK
* PostgreSQL
* Entity Framework Core CLI tools

Verify .NET:

```bash
dotnet --version
```

Verify EF Core:

```bash
dotnet ef --version
```

---

## Clone Repository

```bash
git clone <repository-url>
cd ConferenceRooms
```

---

## Restore Dependencies

```bash
dotnet restore
```

---

## Configure PostgreSQL

Create a database:

```sql
CREATE DATABASE "ConferenceRooms";
```

Configure the connection string in `appsettings.json` or using environment-specific configuration.

---

# Entity Framework Core Migrations

Create a migration:

```bash
dotnet ef migrations add InitialCreate \
    --project ConferenceRooms.Infrastructure \
    --startup-project ConferenceRooms.Api
```

Apply migrations:

```bash
dotnet ef database update \
    --project ConferenceRooms.Infrastructure \
    --startup-project ConferenceRooms.Api
```

The application also runs database migrations through the database seeding process during startup.

---

# Database Seeding

On startup, `DatabaseSeeder`:

1. Applies pending migrations.
2. Checks whether initial data exists.
3. Creates default rooms.
4. Creates default services.
5. Saves changes.

Initial data:

```text
Rooms
├── Room A — 50 seats — 2000 UAH/hour
├── Room B — 100 seats — 3500 UAH/hour
└── Room C — 30 seats — 1500 UAH/hour

Services
├── Projector — 500 UAH
├── Wi-Fi — 300 UAH
└── Sound — 700 UAH
```

The seed is designed to be safe to execute repeatedly without creating duplicate initial data.

---

# Dependency Injection

Application dependencies are registered through:

```csharp
builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);
```

Infrastructure registrations include:

```text
ApplicationDbContext
IRoomRepository      → RoomRepository
IServiceRepository   → ServiceRepository
IBookingRepository   → BookingRepository
```

Application registrations include:

```text
IBookingPriceCalculator → BookingPriceCalculator
```

Repositories and `DbContext` use a scoped lifetime.

The price calculator is stateless and registered as a singleton.

---

# API

The API exposes REST endpoints for:

```text
Rooms
Services
Bookings
Availability
```

Example resource structure:

```text
/api/rooms
/api/services
/api/bookings
```

Typical operations:

```text
POST   /api/rooms
GET    /api/rooms
PUT    /api/rooms/{id}
DELETE /api/rooms/{id}

POST   /api/services
PUT    /api/services/{id}
DELETE /api/services/{id}

POST   /api/bookings
GET    /api/bookings/{id}
PUT    /api/bookings/{id}
DELETE /api/bookings/{id}
```

Availability:

```text
GET /api/rooms/available
```

> Exact routes may depend on the final controller implementation.

---

# Example Booking Request

A booking can contain multiple rooms, with different services assigned to each room.

Example:

```json
{
  "startAt": "2026-09-10T10:00:00+03:00",
  "endAt": "2026-09-10T14:00:00+03:00",
  "rooms": [
    {
      "roomId": "00000000-0000-0000-0000-000000000001",
      "serviceIds": [
        "00000000-0000-0000-0000-000000000101",
        "00000000-0000-0000-0000-000000000102"
      ]
    },
    {
      "roomId": "00000000-0000-0000-0000-000000000002",
      "serviceIds": [
        "00000000-0000-0000-0000-000000000103"
      ]
    }
  ]
}
```

---

# Design Principles

The project follows several architectural principles.

### Separation of Concerns

Each layer has a clearly defined responsibility.

```text
Domain
→ Business rules

Application
→ Use cases

Infrastructure
→ Persistence and external systems

API
→ HTTP transport
```

### Dependency Inversion

Application depends on repository abstractions:

```csharp
IRoomRepository
IServiceRepository
IBookingRepository
```

Infrastructure provides their implementations.

### Rich Domain Model

Business rules are kept inside Domain entities and Domain Services rather than being scattered across controllers or repositories.

### Historical Data Integrity

Booking snapshots prevent changes to current room/service data from affecting historical bookings.

### Async I/O

Database operations use asynchronous APIs and support `CancellationToken`.

---

# Scalability Considerations

The architecture is designed so that individual infrastructure components can evolve without changing the Domain.

Potential future improvements include:

* distributed caching
* Redis
* background processing
* message brokers
* structured logging
* distributed tracing
* authentication and authorization
* rate limiting
* optimistic/pessimistic concurrency
* transaction management
* PostgreSQL JSONB query optimization
* database indexes for availability searches
* normalized booking-room projections for high-volume availability queries
* reporting/read models
* CQRS for read-heavy workloads

---

# Fault Tolerance

Important areas for production hardening include:

* database transactions
* retry policies for transient database failures
* optimistic concurrency
* idempotent commands
* centralized exception handling
* request validation
* structured logging
* health checks
* database connection resilience
* protection against concurrent double booking

In particular, availability checking must eventually be combined with an appropriate concurrency strategy.

A simple:

```text
Check availability
        ↓
Insert booking
```

is not sufficient under high concurrency because two requests can pass the availability check simultaneously.

---

# Project Status

Current implementation:

* [x] Domain entities
* [x] Booking pricing domain service
* [x] Repository abstractions
* [x] Booking commands
* [x] EF Core `DbContext`
* [x] Entity configurations
* [x] PostgreSQL JSONB booking snapshots
* [x] Room repository
* [x] Service repository
* [x] Booking repository
* [x] Infrastructure Dependency Injection
* [x] Database seeding
* [x] EF Core migrations
* [x] API controllers
* [x] Global exception handling
* [ ] Request validation
* [ ] Authentication / Authorization
* [ ] Health checks
* [ ] Automated tests
* [ ] Concurrency protection for double booking
* [ ] Reporting / analytics

---

# License

This project is intended as a technical assignment/demonstration project.
