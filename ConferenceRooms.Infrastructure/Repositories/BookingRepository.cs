using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Entities;
using ConferenceRooms.Domain.Enums;
using ConferenceRooms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Infrastructure.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> HasConfirmedBookingForRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(booking => booking.Status == BookingStatus.Confirmed)
            .ToListAsync(cancellationToken);

        return bookings.Any(booking => booking.Rooms.Any(room => room.RoomId == roomId));
    }

    public async Task<bool> HasOverlappingBookingAsync(
        IDictionary<Guid, (DateTimeOffset StartAt, DateTimeOffset EndAt)> roomReservationPeriods,
        Guid? excludedBookingId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(roomReservationPeriods);

        var requestedRoomIds = roomReservationPeriods.Distinct().ToArray();

        if (requestedRoomIds.Length == 0)
        {
            return false;
        }

        if (requestedRoomIds.Any(x => x.Value.StartAt >= x.Value.EndAt))
        {
            throw new ArgumentException("End time must be greater than start time.");
        }

        var query = _context.Bookings
            .AsNoTracking()
            .Where(booking => booking.Status == BookingStatus.Confirmed);

        if (excludedBookingId.HasValue)
        {
            query = query.Where(x => x.Id != excludedBookingId.Value);
        }

        var bookings = await query.ToListAsync(cancellationToken);

        return bookings.Any(booking => booking.Rooms.Any(bookingRoom => requestedRoomIds.Any(requestedRoom =>
                                                                                    requestedRoom.Key == bookingRoom.RoomId
                                                                                    && requestedRoom.Value.StartAt < bookingRoom.EndAt
                                                                                    && requestedRoom.Value.EndAt > bookingRoom.StartAt)));
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(booking);

        await _context.Bookings.AddAsync(booking, cancellationToken);
    }

    public void Update(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        _context.Bookings.Update(booking);
    }

    public void Delete(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        _context.Bookings.Remove(booking);
    }
}
