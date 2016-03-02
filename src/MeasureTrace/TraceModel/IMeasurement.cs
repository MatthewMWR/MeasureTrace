// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.TraceModel
{
    public interface IMeasurement
    {
        //Trace Trace { get; set; }
        //int TraceId { get; set; }
        MeasurementQuality MeasurementQuality { get; }
        Trace Trace { get; set; }
    }
}