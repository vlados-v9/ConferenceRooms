using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Domain.Contracts;
using ConferenceRooms.Domain.Model;
using System.Collections.Immutable;

namespace ConferenceRooms.Application.Bookings.Commands;

public abstract class BookingHandlerBase
{
    protected readonly IRoomRepository _roomRepository;
    protected readonly IServiceRepository _serviceRepository;
    protected readonly IBookingRepository _bookingRepository;
    protected readonly IBookingPriceCalculator _bookingPriceCalculator;

    public BookingHandlerBase(
        IRoomRepository roomRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository,
        IBookingPriceCalculator bookingPriceCalculator)
    {
        _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
        _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
        _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
        _bookingPriceCalculator = bookingPriceCalculator ?? throw new ArgumentNullException(nameof(bookingPriceCalculator));
    }

    protected async Task<IReadOnlyCollection<BookingRoom>> GetBookingRooms(
        IReadOnlyCollection<BookingRoomRequest> roomsRequest,
        Guid? excludedBookingId,
        CancellationToken cancellationToken)
    {
        if (roomsRequest.Count == 0)
        {
            throw new ArgumentException("At least one room must be selected.", nameof(roomsRequest));
        }

        var roomIds = roomsRequest
            .Select(x => x.RoomId)
            .Distinct()
            .ToArray();

        var rooms = await _roomRepository.GetByIdsAsync(roomIds, cancellationToken);

        if (rooms.Count != roomIds.Length)
        {
            var foundRoomIds = rooms
                .Select(x => x.Id)
                .ToHashSet();

            var missingRoomIds = roomIds
                .Where(x => !foundRoomIds.Contains(x))
                .ToArray();

            throw new KeyNotFoundException($"Rooms not found: {string.Join(", ", missingRoomIds)}");
        }

        if (rooms.Any(x => !x.IsActive))
        {
            throw new InvalidOperationException("One or more selected rooms are inactive.");
        }

        var allServiceIds = roomsRequest
            .SelectMany(x => x.ServiceIds)
            .Distinct()
            .ToArray();

        var services = allServiceIds.Length == 0
            ? []
            : (await _serviceRepository.GetByIdsAsync(
                allServiceIds,
                cancellationToken)).ToArray();

        if (services.Length != allServiceIds.Length)
        {
            var foundServiceIds = services
                .Select(x => x.Id)
                .ToHashSet();

            var missingServiceIds = allServiceIds
                .Where(x => !foundServiceIds.Contains(x))
                .ToArray();

            throw new KeyNotFoundException($"Services not found: {string.Join(", ", missingServiceIds)}");
        }

        if (services.Any(x => !x.IsActive))
        {
            throw new InvalidOperationException("One or more selected services are inactive.");
        }

        var roomReservationPeriods = roomsRequest.ToImmutableDictionary(r => r.RoomId, r => (r.StartAt, r.EndAt));

        var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(
            roomReservationPeriods,
            excludedBookingId: excludedBookingId,
            cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException("One or more selected rooms are already booked for this period.");
        }

        var roomsById = rooms.ToImmutableDictionary(x => x.Id);
        var servicesById = services.ToImmutableDictionary(x => x.Id);

        foreach (var requestRoom in roomsRequest)
        {
            var room = roomsById[requestRoom.RoomId];
            var availableServiceIds = room.Services.Select(x => x.Id).ToHashSet();
            var unavailableServiceIds = requestRoom.ServiceIds
                .Where(serviceId => !availableServiceIds.Contains(serviceId))
                .Distinct()
                .ToArray();

            if (unavailableServiceIds.Length > 0)
            {
                throw new InvalidOperationException($"Services are not available for room '{room.Name}': {string.Join(", ", unavailableServiceIds)}");
            }
        }

        var bookingRooms = new List<BookingRoom>();

        foreach (var requestRoom in roomsRequest)
        {
            var room = roomsById[requestRoom.RoomId];

            var roomServices = requestRoom.ServiceIds
                .Select(serviceId => servicesById[serviceId])
                .ToArray();

            var bookingRoom = BookingRoom.Create(
                room,
                requestRoom.StartAt,
                requestRoom.EndAt,
                roomServices,
                _bookingPriceCalculator);

            bookingRooms.Add(bookingRoom);
        }

        return bookingRooms;
    }
}
