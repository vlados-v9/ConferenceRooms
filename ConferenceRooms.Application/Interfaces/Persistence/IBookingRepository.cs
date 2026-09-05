using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Application.Interfaces.Persistence
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task AddAsync(Booking booking, CancellationToken cancellationToken);

        void Update(Booking booking);

        void Delete(Booking booking);

        Task<bool> HasConfirmedBookingForRoomAsync(
            Guid roomId,
            CancellationToken cancellationToken);

        Task<bool> HasOverlappingBookingAsync(
            IDictionary<Guid, (DateTimeOffset StartAt, DateTimeOffset EndAt)> roomReservationPeriods,
            Guid? excludedBookingId,
            CancellationToken cancellationToken);
    }
}
