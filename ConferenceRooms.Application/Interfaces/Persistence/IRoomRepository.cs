using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Application.Interfaces.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Room>> GetByIdsAsync(
    IEnumerable<Guid> ids,
    CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Room>> GetActiveAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Room>> GetAvailableAsync(
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        int capacity,
        CancellationToken cancellationToken);

    Task AddAsync(Room room, CancellationToken cancellationToken);

    void Update(Room room);

    void Delete(Room room);
}
