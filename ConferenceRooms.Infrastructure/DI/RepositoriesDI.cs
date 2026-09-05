using ConferenceRooms.Application.Interfaces.Persistence;
using ConferenceRooms.Infrastructure.Persistence;
using ConferenceRooms.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConferenceRooms.Infrastructure.DI;

public static class RepositoriesDI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => { options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")); });

        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
