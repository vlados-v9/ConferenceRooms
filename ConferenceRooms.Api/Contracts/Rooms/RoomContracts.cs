namespace ConferenceRooms.Api.Contracts.Rooms;

public sealed class CreateRoomRequest
{
    public string Name { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public decimal BaseHourlyRate { get; init; }

    public IReadOnlyCollection<RoomServiceRequest> Services { get; init; } = [];
}

public sealed class UpdateRoomRequest
{
    public string Name { get; init; } = string.Empty;

    public int Capacity { get; init; }

    public decimal BaseHourlyRate { get; init; }

    public bool IsActive { get; init; } = true;

    public IReadOnlyCollection<RoomServiceRequest> ServicesToAdd { get; init; } = [];
}

public sealed class RoomServiceRequest
{
    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }
}

public sealed record CreatedRoomResponse(Guid Id, string Message);

public sealed record RoomMutationResponse(string Message);
