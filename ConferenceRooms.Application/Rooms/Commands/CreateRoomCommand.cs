namespace ConferenceRooms.Application.Rooms.Commands;

public sealed record CreateRoomCommand(
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    IReadOnlyCollection<RoomServiceInput> Services);
