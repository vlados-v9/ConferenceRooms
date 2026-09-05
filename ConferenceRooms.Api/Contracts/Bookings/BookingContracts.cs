namespace ConferenceRooms.Api.Contracts.Bookings;

public sealed class CreateBookingRequest
{
    public Guid RoomId { get; init; }

    public DateTimeOffset StartAt { get; init; }

    public DateTimeOffset? EndAt { get; init; }

    public decimal? DurationHours { get; init; }

    public IReadOnlyCollection<Guid> ServiceIds { get; init; } = [];
}

public sealed record CreatedBookingResponse(Guid Id, decimal TotalPrice, string Message);

public sealed record BookingMutationResponse(string Message);