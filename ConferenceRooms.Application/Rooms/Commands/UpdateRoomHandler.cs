using ConferenceRooms.Application.Interfaces.Persistence;

namespace ConferenceRooms.Application.Rooms.Commands;

public sealed class UpdateRoomHandler
{
    private readonly IRoomRepository _roomRepository;
    private readonly RoomServiceResolver _roomServiceResolver;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoomHandler(
        IRoomRepository roomRepository,
        RoomServiceResolver roomServiceResolver,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _roomServiceResolver = roomServiceResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        UpdateRoomCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var room = await _roomRepository.GetByIdAsync(command.Id, cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException($"Room '{command.Id}' was not found.");
        }

        room.Update(
            command.Name,
            command.Capacity,
            command.BaseHourlyRate);

        room.ChangeActiveStatus(command.IsActive);

        var services = await _roomServiceResolver.ResolveAsync(command.ServicesToAdd, cancellationToken);

        foreach (var service in services)
        {
            room.AddService(service);
        }

        _roomRepository.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
