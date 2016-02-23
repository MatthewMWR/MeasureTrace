// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.TraceModel
{
    public class ProcessLifetime : MeasurementWithDuration, IMeasurement
    {
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }

        public string ImageName { get; set; }
        public int SessionId { get; set; }
        public Trace Trace { get; set; }
        public int TraceId { get; set; }
    }
}