using ConferenceRooms.Api.Contracts.Rooms;
using ConferenceRooms.Application.Rooms.Commands;
using ConferenceRooms.Application.Rooms.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RoomsController : ControllerBase
{
    private readonly CreateRoomHandler _createRoomHandler;
    private readonly UpdateRoomHandler _updateRoomHandler;
    private readonly DeleteRoomHandler _deleteRoomHandler;
    private readonly SearchAvailableRoomsHandler _searchAvailableRoomsHandler;

    public RoomsController(
        CreateRoomHandler createRoomHandler,
        UpdateRoomHandler updateRoomHandler,
        DeleteRoomHandler deleteRoomHandler,
        SearchAvailableRoomsHandler searchAvailableRoomsHandler)
    {
        _createRoomHandler = createRoomHandler ?? throw new ArgumentNullException(nameof(createRoomHandler));
        _updateRoomHandler = updateRoomHandler ?? throw new ArgumentNullException(nameof(updateRoomHandler));
        _deleteRoomHandler = deleteRoomHandler ?? throw new ArgumentNullException(nameof(deleteRoomHandler));
        _searchAvailableRoomsHandler = searchAvailableRoomsHandler ?? throw new ArgumentNullException(nameof(searchAvailableRoomsHandler));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreatedRoomResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedRoomResponse>> Create(
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var roomId = await _createRoomHandler.HandleAsync(
            new CreateRoomCommand(
                request.Name,
                request.Capacity,
                request.BaseHourlyRate,
                MapServices(request.Services)),
            cancellationToken);

        return Created(
            $"/api/rooms/{roomId}",
            new CreatedRoomResponse(roomId, "Зал успішно створено."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoomMutationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomMutationResponse>> Update(
        Guid id,
        [FromBody] UpdateRoomRequest request,
        CancellationToken cancellationToken)
    {
        await _updateRoomHandler.HandleAsync(
            new UpdateRoomCommand(
                id,
                request.Name,
                request.Capacity,
                request.BaseHourlyRate,
                request.IsActive,
                MapServices(request.ServicesToAdd)),
            cancellationToken);

        return Ok(new RoomMutationResponse("Інформацію про зал успішно оновлено."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(RoomMutationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RoomMutationResponse>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _deleteRoomHandler.HandleAsync(new DeleteRoomCommand(id), cancellationToken);

        return Ok(new RoomMutationResponse("Зал успішно видалено."));
    }

    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AvailableRoomDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AvailableRoomDto>>> SearchAvailable(
        [FromQuery] int capacity,
        [FromQuery] DateTimeOffset? startAt,
        [FromQuery] DateTimeOffset? endAt,
        [FromQuery] DateOnly? date,
        [FromQuery] TimeOnly? from,
        [FromQuery] TimeOnly? to,
        CancellationToken cancellationToken)
    {
        var period = ResolvePeriod(startAt, endAt, date, from, to);

        var rooms = await _searchAvailableRoomsHandler.HandleAsync(
            new SearchAvailableRoomsQuery(period.StartAt, period.EndAt, capacity),
            cancellationToken);

        return Ok(rooms);
    }

    private static IReadOnlyCollection<RoomServiceInput> MapServices(IReadOnlyCollection<RoomServiceRequest>? services) =>
        (services ?? [])
            .Select(service => new RoomServiceInput(service.Name, service.Price))
            .ToArray();

    private static (DateTimeOffset StartAt, DateTimeOffset EndAt) ResolvePeriod(
        DateTimeOffset? startAt,
        DateTimeOffset? endAt,
        DateOnly? date,
        TimeOnly? from,
        TimeOnly? to)
    {
        if (startAt.HasValue && endAt.HasValue)
        {
            return (startAt.Value, endAt.Value);
        }

        if (date.HasValue && from.HasValue && to.HasValue)
        {
            return (
                new DateTimeOffset(date.Value.ToDateTime(from.Value), TimeSpan.Zero),
                new DateTimeOffset(date.Value.ToDateTime(to.Value), TimeSpan.Zero));
        }

        throw new ArgumentException("Specify startAt and endAt, or date with from and to.");
    }
}
