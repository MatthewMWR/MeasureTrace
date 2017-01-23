//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public class ProcessLifetime : MeasurementWithDuration, IMeasurement
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
        public int SessionId { get; set; }
        public Trace Trace { get; set; }
    }
}