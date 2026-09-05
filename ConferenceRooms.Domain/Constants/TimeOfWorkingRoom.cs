namespace ConferenceRooms.Domain.Constants
{
    public static class TimeOfWorkingRoom
    {
        public const decimal MorningMultiplier = 0.90m;
        public const decimal StandardMultiplier = 1.00m;
        public const decimal PeakMultiplier = 1.15m;
        public const decimal EveningMultiplier = 0.80m;

        public static readonly TimeSpan MorningStart = new(6, 0, 0);
        public static readonly TimeSpan MorningEnd = new(9, 0, 0);
        public static readonly TimeSpan StandardStart = new(9, 0, 0);
        public static readonly TimeSpan StandardEnd = new(18, 0, 0);
        public static readonly TimeSpan PeakStart = new(12, 0, 0);
        public static readonly TimeSpan PeakEnd = new(14, 0, 0);
        public static readonly TimeSpan EveningStart = new(18, 0, 0);
        public static readonly TimeSpan EveningEnd = new(23, 0, 0);
    }
}
