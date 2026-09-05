namespace ConferenceRooms.Domain.Entities;

public class Service
{
    private Service()
    {
    }

    private Service(
        Guid id,
        string name,
        decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public decimal Price { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static Service Create(string name, decimal price)
    {
        Validate(name, price);

        return new Service(
            Guid.NewGuid(),
            name.Trim(),
            price);
    }

    public void Update(string name, decimal price)
    {
        Validate(name, price);

        Name = name.Trim();
        Price = price;
    }

    public void ChangeActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }

    private static void Validate(
        string name,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Service name cannot be empty.",
                nameof(name));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Service price cannot be negative.");
        }
    }
}
