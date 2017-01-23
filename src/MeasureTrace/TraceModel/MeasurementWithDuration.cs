//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using MeasureTrace.Calipers;

namespace MeasureTrace.TraceModel
{
    public abstract class MeasurementWithDuration
    {
        public double? DurationMSec { get; set; }
        public double? DurationSeconds => DurationMSec/1000;
        public virtual MeasurementQuality MeasurementQuality => MeasurementExtensions.ResolveQuality(this);
    }
}