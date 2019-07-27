namespace Net.Ntp
{
    public enum LeapIndicator
    {
        NoLeapSecondAdjustment,
        LastMinuteHas61Seconds,
        LastMinuteHas59Seconds,
        ClockUnsynchronized
    }
}
