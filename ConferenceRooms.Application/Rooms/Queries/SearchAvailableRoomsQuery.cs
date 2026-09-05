namespace ConferenceRooms.Application.Rooms.Queries;

public sealed record SearchAvailableRoomsQuery(
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    int Capacity);
