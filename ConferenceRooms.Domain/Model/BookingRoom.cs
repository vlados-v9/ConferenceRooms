using ConferenceRooms.Domain.Contracts;
using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Domain.Model;

public sealed class BookingRoom
{
    private BookingRoom()
    {
    }

    private BookingRoom(
        Guid roomId,
        string name,
        int capacity,
        decimal hourlyRate,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        IReadOnlyCollection<BookingService> services,
        IBookingPriceCalculator bookingPriceCalculator)
    {
        RoomId = roomId;
        Name = name;
        Capacity = capacity;
        HourlyRate = hourlyRate;
        Services = services;
        StartAt = startAt;
        EndAt = endAt;

        ServicesPrice = services.Sum(x => x.Price);
        RentalPrice = bookingPriceCalculator.CalculateRoomRentalPrice(startAt, endAt, hourlyRate);
        TotalPrice = RentalPrice + ServicesPrice;
    }

    public Guid RoomId { get; init; }

    public string Name { get; init; } = null!;

    public int Capacity { get; init; }

    public decimal HourlyRate { get; init; }

    public decimal RentalPrice { get; private set; }

    public decimal ServicesPrice { get; init; }

    public decimal TotalPrice { get; init; }

    public DateTimeOffset StartAt { get; init; }

    public DateTimeOffset EndAt { get; init; }

    public IReadOnlyCollection<BookingService> Services { get; init; } = [];

    public static BookingRoom Create(
        Room room,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        IEnumerable<Service> services,
        IBookingPriceCalculator bookingPriceCalculator)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(bookingPriceCalculator);

        var bookingServices = services
            .Select(BookingService.Create)
            .ToList();

        return new BookingRoom(
            room.Id,
            room.Name,
            room.Capacity,
            room.BaseHourlyRate,
            startAt,
            endAt,
            bookingServices,
            bookingPriceCalculator);
    }
}
