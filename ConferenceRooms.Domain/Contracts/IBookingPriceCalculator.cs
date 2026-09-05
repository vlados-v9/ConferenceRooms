namespace ConferenceRooms.Domain.Contracts
{
    public interface IBookingPriceCalculator
    {
        decimal CalculateRoomRentalPrice(
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            decimal hourlyRate);
    }
}
