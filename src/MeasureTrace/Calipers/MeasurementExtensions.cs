//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using MeasureTrace.Adapters;
using MeasureTrace.TraceModel;

namespace MeasureTrace.Calipers
{
    public static class MeasurementExtensions
    {
        public static MeasurementQuality ResolveQuality(MeasurementWithDuration measurement)
        {
            if (measurement == null) throw new ArgumentNullException(nameof(measurement));
            if (measurement.DurationMSec < 0) return MeasurementQuality.Unreliable;
            if (measurement.DurationMSec == WptInterop.WptMagicNumberNullPostBootDuration)
                return MeasurementQuality.Unreliable;
            if (measurement.DurationMSec > int.MaxValue) return MeasurementQuality.Unreliable;
            if (measurement.DurationMSec == WptInterop.WptMagicNumberUnboundDuration)
                return MeasurementQuality.Unreliable;
            return MeasurementQuality.DefaultUsable;
        }
    }
}