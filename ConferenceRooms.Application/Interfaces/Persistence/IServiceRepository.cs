using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Application.Interfaces.Persistence
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<Service>> GetByIdsAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken);

        Task AddAsync(Service service, CancellationToken cancellationToken);

        void Update(Service service);

        void Delete(Service service);
    }
}
