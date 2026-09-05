using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Contracts;

namespace ConferenceRooms.Application.Bookings.Commands;

public sealed class UpdateBookingHandler : BookingHandlerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBookingHandler(
        IRoomRepository roomRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository,
        IBookingPriceCalculator bookingPriceCalculator,
        IUnitOfWork unitOfWork)
        : base(roomRepository, serviceRepository, bookingRepository, bookingPriceCalculator)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task HandleAsync(UpdateBookingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var booking = await _bookingRepository.GetByIdAsync(command.Id, cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking '{command.Id}' was not found.");
        }

        var roomsRequest = command.Rooms
            .Select(room => room with
            {
                StartAt = room.StartAt.ToUniversalTime(),
                EndAt = room.EndAt.ToUniversalTime()
            })
            .ToArray();

        var bookingRooms = await GetBookingRooms(roomsRequest, booking.Id, cancellationToken);

        booking.UpdateRooms(bookingRooms);

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
