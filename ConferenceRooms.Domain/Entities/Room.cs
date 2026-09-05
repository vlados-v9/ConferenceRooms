namespace ConferenceRooms.Domain.Entities;

public class Room
{
    private readonly List<Service> _services = [];

    private Room()
    {
    }

    private Room(
        Guid id,
        string name,
        int capacity,
        decimal baseHourlyRate)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public bool IsActive { get; private set; } = true;

    public string Name { get; private set; } = null!;

    public int Capacity { get; private set; }

    public decimal BaseHourlyRate { get; private set; }

    public IReadOnlyCollection<Service> Services =>
        _services.AsReadOnly();

    public static Room Create(string name, int capacity, decimal baseHourlyRate)
    {
        Validate(name, capacity, baseHourlyRate);

        return new Room(
            Guid.NewGuid(),
            name.Trim(),
            capacity,
            baseHourlyRate);
    }

    public void ChangeActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }

    public void Update(string name, int capacity, decimal baseHourlyRate)
    {
        Validate(name, capacity, baseHourlyRate);

        Name = name.Trim();
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
    }

    public void AddService(Service service)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (_services.Any(x => x.Id == service.Id))
        {
            return;
        }

        _services.Add(service);
    }

    public void RemoveService(Guid serviceId)
    {
        var service = _services.FirstOrDefault(
            x => x.Id == serviceId);

        if (service is not null)
        {
            _services.Remove(service);
        }
    }

    private static void Validate(string name, int capacity, decimal baseHourlyRate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Room name cannot be empty.",
                nameof(name));
        }

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Room capacity must be greater than zero.");
        }

        if (baseHourlyRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(baseHourlyRate),
                "Base hourly rate must be greater than zero.");
        }
    }
}
