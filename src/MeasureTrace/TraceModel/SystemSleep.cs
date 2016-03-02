// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.TraceModel
{
    public class SystemSleep : MeasurementWithDuration, IMeasurement
    {
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }

        //  TODO FUTURE
        //  Currently each measurement class has to implement MeasurementQuality, even when it is just a pass through as it is here.
        //  This is for compatibility with a bug in EF7
        //  Hopefully these can be removed in the future.
        public Trace Trace { get; set; }
        //public int TraceId { get; set; }
    }
}