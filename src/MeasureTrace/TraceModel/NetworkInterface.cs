//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public class NetworkInterface : IMeasurement
    {
        public int Id { get; set; }
        public string IpAddressesFlat { get; set; }
        public string DnsServersFlat { get; set; }
        public string DefaultGateway { get; set; }
        public string SubnetId { get; set; }
        public string Description { get; set; }
        public Trace Trace { get; set; }
        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;
    }
}