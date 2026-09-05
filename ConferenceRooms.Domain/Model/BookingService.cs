using ConferenceRooms.Domain.Entities;

namespace ConferenceRooms.Domain.Model;

public sealed class BookingService
{
    private BookingService()
    {
    }

    private BookingService(
        Guid serviceId,
        string name,
        decimal price)
    {
        ServiceId = serviceId;
        Name = name;
        Price = price;
    }

    public Guid ServiceId { get; init; }

    public string Name { get; init; } = null!;

    public decimal Price { get; init; }

    public static BookingService Create(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);

        return new BookingService(
            service.Id,
            service.Name,
            service.Price);
    }
}
