using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Application.Rooms.Commands;

public sealed class RoomServiceResolver
{
    private readonly IServiceRepository _serviceRepository;

    public RoomServiceResolver(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<IReadOnlyCollection<Service>> ResolveAsync(
        IReadOnlyCollection<RoomServiceInput>? services,
        CancellationToken cancellationToken)
    {
        if (services is null || services.Count == 0)
        {
            return [];
        }

        var resolved = new List<Service>();

        foreach (var input in services)
        {
            var service = await _serviceRepository.GetByNameAsync(input.Name, cancellationToken);

            if (service is null)
            {
                service = Service.Create(input.Name, input.Price);
                await _serviceRepository.AddAsync(service, cancellationToken);
            }

            resolved.Add(service);
        }

        return resolved;
    }
}
