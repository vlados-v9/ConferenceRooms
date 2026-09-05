namespace ConferenceRooms.Application.Rooms.Commands;

public sealed record UpdateRoomCommand(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    bool IsActive,
    IReadOnlyCollection<RoomServiceInput> ServicesToAdd);
