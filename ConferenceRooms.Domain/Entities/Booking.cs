using ConferenceRooms.Domain.Enums;
using ConferenceRooms.Domain.Model;

namespace ConferenceRooms.Domain.Entities;

public class Booking
{
    private readonly List<BookingRoom> _rooms = [];

    private Booking()
    {
    }

    private Booking(
        Guid id,
        IEnumerable<BookingRoom> rooms)
    {
        Id = id;
        Status = BookingStatus.Confirmed;

        _rooms.AddRange(rooms);
    }

    public Guid Id { get; private set; }

    public BookingStatus Status { get; private set; }

    public decimal TotalPrice { get; private set; }

    public IReadOnlyCollection<BookingRoom> Rooms => _rooms.AsReadOnly();

    public static Booking Create(
        IEnumerable<BookingRoom> rooms)
    {
        ArgumentNullException.ThrowIfNull(rooms);

        var bookingRooms = rooms.ToList();

        ValidateRooms(bookingRooms);

        var booking = new Booking(Guid.NewGuid(), bookingRooms);

        booking.TotalPrice = rooms.Sum(r => r.TotalPrice);

        return booking;
    }

    public void UpdateRooms(IEnumerable<BookingRoom> rooms)
    {
        EnsureCanBeModified();

        ArgumentNullException.ThrowIfNull(rooms);

        var bookingRooms = rooms.ToList();

        ValidateRooms(bookingRooms);

        _rooms.Clear();
        _rooms.AddRange(bookingRooms);

        TotalPrice = bookingRooms.Sum(r => r.TotalPrice);
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            return;
        }

        if (Status == BookingStatus.Completed)
        {
            throw new InvalidOperationException("Completed booking cannot be cancelled.");
        }

        Status = BookingStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed bookings can be completed.");
        }

        Status = BookingStatus.Completed;
    }

    private void EnsureCanBeModified()
    {
        if (Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed bookings can be modified.");
        }
    }

    private static void ValidateRooms(IReadOnlyCollection<BookingRoom> rooms)
    {
        if (rooms.Count == 0)
        {
            throw new ArgumentException("Booking must contain at least one room.");
        }

        var hasDuplicates = rooms
            .GroupBy(x => new { x.RoomId, x.StartAt })
            .Any(x => x.Count() > 1);

        if (hasDuplicates)
        {
            throw new ArgumentException("Booking cannot contain the same room more than once.");
        }

        foreach (var room in rooms)
        {
            ValidatePeriod(room.StartAt, room.EndAt);
        }
    }

    private static void ValidatePeriod(DateTimeOffset startAt, DateTimeOffset endAt)
    {
        if (endAt <= startAt)
        {
            throw new ArgumentException("Booking end time must be greater than start time.");
        }
    }
}
