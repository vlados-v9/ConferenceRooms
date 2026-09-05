using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Application.Rooms.Commands;

public sealed class CreateRoomHandler
{
    private readonly IRoomRepository _roomRepository;
    private readonly RoomServiceResolver _roomServiceResolver;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoomHandler(
        IRoomRepository roomRepository,
        RoomServiceResolver roomServiceResolver,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _roomServiceResolver = roomServiceResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(
        CreateRoomCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var room = Room.Create(
            command.Name,
            command.Capacity,
            command.BaseHourlyRate);

        var services = await _roomServiceResolver.ResolveAsync(command.Services, cancellationToken);

        foreach (var service in services)
        {
            room.AddService(service);
        }

        await _roomRepository.AddAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return room.Id;
    }
}
