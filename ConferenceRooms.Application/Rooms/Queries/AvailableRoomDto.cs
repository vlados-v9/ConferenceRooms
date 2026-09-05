namespace ConferenceRooms.Application.Rooms.Queries;

public sealed record AvailableRoomDto(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    IReadOnlyCollection<RoomServiceDto> Services);

public sealed record RoomServiceDto(
    Guid Id,
    string Name,
    decimal Price);
