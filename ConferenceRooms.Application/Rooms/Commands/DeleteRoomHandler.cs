using ConferenceRooms.Application.Interfaces.Persistence;

namespace ConferenceRooms.Application.Rooms.Commands;

public sealed class DeleteRoomHandler
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoomHandler(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var room = await _roomRepository.GetByIdAsync(command.Id, cancellationToken);

        if (room is null)
        {
            throw new KeyNotFoundException($"Room '{command.Id}' was not found.");
        }

        var hasBookings = await _bookingRepository.HasConfirmedBookingForRoomAsync(command.Id, cancellationToken);

        if (hasBookings)
        {
            throw new InvalidOperationException("Room cannot be deleted while it has confirmed bookings.");
        }

        _roomRepository.Delete(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
