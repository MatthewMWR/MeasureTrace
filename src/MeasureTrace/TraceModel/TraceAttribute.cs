// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;

namespace MeasureTrace.TraceModel
{
    public class TraceAttribute : IMeasurement
    {
        public string Name { get; set; }
        public string StringValue { get; set; }
        public int? WholeNumberValue { get; set; }
        public double? DecimalValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public int Id { get; set; }
        public Trace Trace { get; set; }
        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;
#pragma warning disable 169
        // dummy "Backing field" for EF compat with no-setter properties
        private string _measurementQuality;
#pragma warning restore 169
    }
}