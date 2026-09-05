using ConferenceRooms.Api.Contracts.Bookings;
using ConferenceRooms.Application.Bookings.Commands;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BookingsController : ControllerBase
{
    private readonly CreateBookingHandler _createBookingHandler;
    private readonly CancelBookingHandler _cancelBookingHandler;
    private readonly UpdateBookingHandler _updateBookingHandler;

    public BookingsController(
        CreateBookingHandler createBookingHandler,
        CancelBookingHandler cancelBookingHandler,
        UpdateBookingHandler updateBookingHandler)
    {
        _createBookingHandler = createBookingHandler ?? throw new ArgumentNullException(nameof(createBookingHandler));
        _cancelBookingHandler = cancelBookingHandler ?? throw new ArgumentNullException(nameof(cancelBookingHandler));
        _updateBookingHandler = updateBookingHandler ?? throw new ArgumentNullException(nameof(updateBookingHandler));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreatedBookingResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedBookingResponse>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var endAt = ResolveEndAt(request);

        var result = await _createBookingHandler.HandleAsync(
            new CreateBookingCommand(
            [
                new BookingRoomRequest(
                    request.RoomId,
                    request.StartAt,
                    endAt,
                    request.ServiceIds ?? [])
            ]),
            cancellationToken);

        return Created(
            $"/api/bookings/{result.Id}",
            new CreatedBookingResponse(
                result.Id,
                result.TotalPrice,
                "Бронювання підтверджено."));
    }

    [HttpPut("cancel")]
    [ProducesResponseType(typeof(BookingMutationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BookingMutationResponse>> CancelBooking(
        [FromQuery] Guid id,
        CancellationToken cancellationToken)
    {
        await _cancelBookingHandler.HandleAsync(new CancelBookingCommand(id), cancellationToken);

        return Ok(new BookingMutationResponse("Бронювання скасовано."));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookingMutationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BookingMutationResponse>> UpdateBooking(
        Guid id,
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var endAt = ResolveEndAt(request);

        await _updateBookingHandler.HandleAsync(new UpdateBookingCommand(id,
            [
                new BookingRoomRequest(
                    request.RoomId,
                    request.StartAt,
                    endAt,
                    request.ServiceIds ?? [])
            ]), cancellationToken);

        return Ok(new BookingMutationResponse("Бронювання оновлено."));
    }

    private static DateTimeOffset ResolveEndAt(CreateBookingRequest request)
    {
        if (request.EndAt.HasValue)
        {
            return request.EndAt.Value;
        }

        if (request.DurationHours is > 0)
        {
            return request.StartAt.AddHours((double)request.DurationHours.Value);
        }

        throw new ArgumentException("Specify endAt or a positive durationHours.");
    }
}
