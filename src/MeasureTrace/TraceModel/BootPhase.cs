//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using MeasureTrace.Calipers;

namespace MeasureTrace.TraceModel
{
    public class BootPhase : MeasurementWithDuration, IMeasurement
    {
        public int Id { get; set; }

        public BootPhaseObserver BootPhaseObserver { get; set; }

        public BootPhaseType BootPhaseType { get; set; }

        public Trace Trace { get; set; }

        public override MeasurementQuality MeasurementQuality => MeasurementExtensions.ResolveQuality(this);
    }
}