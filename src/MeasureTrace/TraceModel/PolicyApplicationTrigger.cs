namespace MeasureTrace.TraceModel
{
    public enum PolicyApplicationTrigger
    {
        None = 0,
        Boot,
        LogOn,
        NetworkStateChange,
        Manual,
        Periodic,
        Invalid
    }
}