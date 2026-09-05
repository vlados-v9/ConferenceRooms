using ConferenceRooms.Domain.Entities;
using ConferenceRooms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await SeedAsyncInternal(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAsyncInternal(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Rooms.AnyAsync(cancellationToken) || await context.Services.AnyAsync(cancellationToken))
        {
            return;
        }

        var projector = Service.Create(name: "Projector", price: 500m);
        var wifi = Service.Create(name: "Wi-Fi", price: 300m);
        var sound = Service.Create(name: "Sound", price: 700m);

        var roomA = Room.Create(name: "Room A", capacity: 50, baseHourlyRate: 2000m);
        roomA.AddService(projector);
        roomA.AddService(wifi);

        var roomB = Room.Create(name: "Room B", capacity: 100, baseHourlyRate: 3500m);
        roomB.AddService(projector);
        roomB.AddService(wifi);
        roomB.AddService(sound);

        var roomC = Room.Create(name: "Room C", capacity: 30, baseHourlyRate: 1500m);
        roomC.AddService(wifi);

        await context.Services.AddRangeAsync([projector, wifi, sound], cancellationToken);
        await context.Rooms.AddRangeAsync([roomA, roomB, roomC], cancellationToken);
    }
}