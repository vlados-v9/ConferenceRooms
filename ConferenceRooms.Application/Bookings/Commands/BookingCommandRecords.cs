namespace ConferenceRooms.Application.Bookings.Commands;

public sealed record CreateBookingCommand(IReadOnlyCollection<BookingRoomRequest> Rooms);

public sealed record CreateBookingResult(Guid Id, decimal TotalPrice);

public sealed record UpdateBookingCommand(Guid Id, IReadOnlyCollection<BookingRoomRequest> Rooms);

public sealed record CancelBookingCommand(Guid Id);

public sealed record BookingRoomRequest(
    Guid RoomId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    IReadOnlyCollection<Guid> ServiceIds);