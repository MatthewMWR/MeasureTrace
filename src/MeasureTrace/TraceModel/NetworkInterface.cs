﻿// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.TraceModel
{
    public class NetworkInterface : IMeasurement
    {
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }
        public string IpAddressesFlat { get; set; }
        public string DnsServersFlat { get; set; }
        public string DefaultGateway { get; set; }
        public string SubnetId { get; set; }
        public string Description { get; set; }
        public Trace Trace { get; set; }
        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;
    }
}