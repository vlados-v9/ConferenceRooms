using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Entities;
using ConferenceRooms.Domain.Enums;
using ConferenceRooms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Infrastructure.Repositories;

public sealed class RoomRepository : IRoomRepository
{
    private readonly ApplicationDbContext _context;

    public RoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Rooms
            .Include(x => x.Services)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Room>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var roomIds = ids.Distinct().ToArray();

        if (roomIds.Length == 0) return [];

        return await _context.Rooms
            .Include(x => x.Services)
            .Where(x => roomIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Room>> GetAvailableAsync(
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        int capacity,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        if (endAt <= startAt)
        {
            throw new ArgumentException("End time must be greater than start time.");
        }

        return await _context.Rooms
            .Include(x => x.Services)
            .Where(x => x.IsActive && x.Capacity >= capacity)
            .Where(room => !_context.Bookings.Any(booking => booking.Status == BookingStatus.Confirmed
                                                             && booking.Rooms.Any(bookingRoom => bookingRoom.RoomId == room.Id
                                                                                                 && bookingRoom.StartAt < endAt
                                                                                                 && bookingRoom.EndAt > startAt)))
            .OrderBy(x => x.Capacity)
            .ThenBy(x => x.BaseHourlyRate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Room>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await _context.Rooms.Where(x => x.IsActive).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Room room, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(room);
        await _context.Rooms.AddAsync(room, cancellationToken);
    }

    public void Update(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        _context.Rooms.Update(room);
    }

    public void Delete(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        _context.Rooms.Remove(room);
    }
}
