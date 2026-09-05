using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Entities;
using ConferenceRooms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Infrastructure.Repositories;

public sealed class ServiceRepository : IServiceRepository
{
    private readonly ApplicationDbContext _context;

    public ServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _context.Services.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Service?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.Trim().ToLowerInvariant();

        return await _context.Services.FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Service>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var serviceIds = ids.Distinct().ToArray();

        if (serviceIds.Length == 0) return [];

        return await _context.Services.Where(x => serviceIds.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Service service, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(service);

        await _context.Services.AddAsync(service, cancellationToken);
    }

    public void Update(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);

        _context.Services.Update(service);
    }

    public void Delete(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);

        _context.Services.Remove(service);
    }
}
