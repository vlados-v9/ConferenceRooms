using ConferenceRooms.Application.Interfaces.Persistence;

namespace ConferenceRooms.Application.Bookings.Commands;

public sealed class CancelBookingHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var booking = await _bookingRepository.GetByIdAsync(command.Id, cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking '{command.Id}' was not found.");
        }

        booking.Cancel();

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
