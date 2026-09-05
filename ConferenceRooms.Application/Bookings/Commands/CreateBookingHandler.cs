using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Contracts;
using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Application.Bookings.Commands;

public sealed class CreateBookingHandler : BookingHandlerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingHandler(
        IRoomRepository roomRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository,
        IBookingPriceCalculator bookingPriceCalculator,
        IUnitOfWork unitOfWork)
        : base(roomRepository, serviceRepository, bookingRepository, bookingPriceCalculator)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateBookingResult> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var roomsRequest = command.Rooms
            .Select(room => room with
            {
                StartAt = room.StartAt.ToUniversalTime(),
                EndAt = room.EndAt.ToUniversalTime()
            })
            .ToArray();

        var bookingRooms = await GetBookingRooms(roomsRequest, excludedBookingId: null, cancellationToken);

        var booking = Booking.Create(bookingRooms);

        await _bookingRepository.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingResult(booking.Id, booking.TotalPrice);
    }
}
