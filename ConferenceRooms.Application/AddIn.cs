using ConferenceRooms.Application.Bookings.Commands;
using ConferenceRooms.Application.Rooms.Commands;
using ConferenceRooms.Application.Rooms.Queries;
using ConferenceRooms.Domain.Contracts;
using ConferenceRooms.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRooms.Application;

public static class AddIn
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IBookingPriceCalculator, BookingPriceCalculator>();

        services.AddScoped<RoomServiceResolver>();
        services.AddScoped<CreateRoomHandler>();
        services.AddScoped<UpdateRoomHandler>();
        services.AddScoped<DeleteRoomHandler>();
        services.AddScoped<SearchAvailableRoomsHandler>();

        services.AddScoped<CreateBookingHandler>();
        services.AddScoped<UpdateBookingHandler>();
        services.AddScoped<CancelBookingHandler>();

        return services;
    }
}
