// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace MeasureTrace.Calipers
{
    public class NetworkInterfaceProcessor : ProcessorBase, IObserver<SystemConfigNICTraceData>
    {
        private readonly string[] _iPListSplitter = {";"};

        public void OnNext(SystemConfigNICTraceData sysConfigNic)
        {
            var newNic = new NetworkInterface();
            newNic.IpAddressesFlat = sysConfigNic.IpAddresses;
            newNic.DnsServersFlat = sysConfigNic.DnsServerAddresses;
            newNic.Description = sysConfigNic.NICDescription;
            RegisterMeasurement(newNic);
        }

        public void OnCompleted()
        {
        }

        public override void Initialize(TraceJob traceJob)
        {
            Subscriptions.Add(traceJob.EtwTraceEventSource.Kernel.Observe<SystemConfigNICTraceData>().Subscribe(this));
        }
    }
}