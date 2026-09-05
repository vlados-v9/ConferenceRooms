using ConferenceRooms.Domain.Constants;
using ConferenceRooms.Domain.Contracts;

namespace ConferenceRooms.Domain.Services;

public sealed class BookingPriceCalculator : IBookingPriceCalculator
{
    public decimal CalculateRoomRentalPrice(
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        decimal hourlyRate)
    {
        if (endAt <= startAt)
        {
            throw new ArgumentException("Booking end time must be greater than start time.");
        }

        var duration = endAt - startAt;

        var multiplier = GetMultiplier(startAt.TimeOfDay, endAt.TimeOfDay);

        var hours = (decimal)duration.TotalHours;

        var total = hourlyRate * hours * multiplier;

        return total;
    }

    private static decimal GetMultiplier(TimeSpan startTime, TimeSpan endTime)
    {
        if (startTime >= TimeOfWorkingRoom.PeakStart && endTime <= TimeOfWorkingRoom.PeakEnd)
        {
            return TimeOfWorkingRoom.PeakMultiplier;
        }

        if (startTime >= TimeOfWorkingRoom.MorningStart && endTime <= TimeOfWorkingRoom.MorningEnd)
        {
            return TimeOfWorkingRoom.MorningMultiplier;
        }

        if (startTime >= TimeOfWorkingRoom.EveningStart && endTime <= TimeOfWorkingRoom.EveningEnd)
        {
            return TimeOfWorkingRoom.EveningMultiplier;
        }

        if (startTime >= TimeOfWorkingRoom.StandardStart && endTime <= TimeOfWorkingRoom.StandardEnd)
        {
            return TimeOfWorkingRoom.StandardMultiplier;
        }

        return 1.0m;
    }
}
