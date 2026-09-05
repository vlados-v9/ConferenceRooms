using ConferenceRooms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRooms.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Capacity).IsRequired();

        builder.Property(x => x.BaseHourlyRate)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.IsActive).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasIndex(x => new
        {
            x.IsActive,
            x.Capacity
        });

        builder.HasMany(x => x.Services)
            .WithMany()
            .UsingEntity(join => join.ToTable("RoomServices"));

        builder.Navigation(x => x.Services)
            .HasField("_services")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Room_Capacity_Positive",
                "\"Capacity\" > 0");

            table.HasCheckConstraint(
                "CK_Room_BaseHourlyRate_NonNegative",
                "\"BaseHourlyRate\" >= 0");
        });
    }
}
