// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using MeasureTrace.Calipers;

namespace MeasureTrace.TraceModel
{
    public class BootPhase : MeasurementWithDuration, IMeasurement
    {
        public int Id { get; set; }

        public BootPhaseObserver BootPhaseObserver { get; set; }

        public string BootPhaseObserverLabel => Enum.GetName(typeof (BootPhaseObserver), BootPhaseObserver);

        public BootPhaseType BootPhaseType { get; set; }

        public string BootPhaseTypeLabel => Enum.GetName(typeof (BootPhaseType), BootPhaseType);
        public Trace Trace { get; set; }
        //public int TraceId { get; set; }

        public override MeasurementQuality MeasurementQuality => MeasurementExtensions.ResolveQuality(this);
    }
}