using ConferenceRooms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRooms.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.OwnsMany(x => x.Rooms,
            roomBuilder =>
            {
                roomBuilder.ToJson("RoomsSnapshot");

                roomBuilder.Property(x => x.HourlyRate).HasPrecision(18, 2);
                roomBuilder.Property(x => x.RentalPrice).HasPrecision(18, 2);
                roomBuilder.Property(x => x.ServicesPrice).HasPrecision(18, 2);
                roomBuilder.Property(x => x.TotalPrice).HasPrecision(18, 2);

                roomBuilder.OwnsMany(x => x.Services,
                    serviceBuilder =>
                    {
                        serviceBuilder.Property(x => x.Price).HasPrecision(18, 2);
                    });
            });


        builder.HasIndex(x => x.Status);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Booking_TotalPrice_NonNegative",
                "\"TotalPrice\" >= 0");
        });
    }
}
