using ConferenceRooms.Application.Interfaces.Persistence;

namespace ConferenceRooms.Application.Rooms.Queries;

public sealed class SearchAvailableRoomsHandler
{
    private readonly IRoomRepository _roomRepository;

    public SearchAvailableRoomsHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<IReadOnlyCollection<AvailableRoomDto>> HandleAsync(
        SearchAvailableRoomsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rooms = await _roomRepository.GetAvailableAsync(
            query.StartAt.ToUniversalTime(),
            query.EndAt.ToUniversalTime(),
            query.Capacity,
            cancellationToken);

        return rooms
            .Select(room => new AvailableRoomDto(
                room.Id,
                room.Name,
                room.Capacity,
                room.BaseHourlyRate,
                room.Services
                    .Where(service => service.IsActive)
                    .Select(service => new RoomServiceDto(service.Id, service.Name, service.Price))
                    .ToArray()))
            .ToArray();
    }
}
