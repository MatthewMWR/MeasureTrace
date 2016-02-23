// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;
#pragma warning disable 618

namespace MeasureTrace.Calipers
{
    /// <summary>
    ///     At present the SystemSleepProcessor and SystemSleep measurement exist only to inform whether a sleep
    ///     occurred during the trace as that may throw off other measurements
    /// </summary>
    public class SystemSleepProcessor : ProcessorBase, IObserver<TraceEvent>
    {
        private const string WindowsKernelPowerProviderName = "Microsoft-Windows-Kernel-Power";

        public void OnNext(TraceEvent value)
        {
            if ((int) value.ID == 42)
            {
                RegisterMeasurement(new SystemSleep());
            }
        }

        public void OnCompleted()
        {
        }

        public override void Initialize(TraceJob traceJob)
        {
            Subscriptions.Add(
                traceJob.EtwTraceEventSource.Registered.Observe(
                    (providerName, eventName) =>
                        string.Equals(providerName, WindowsKernelPowerProviderName, StringComparison.OrdinalIgnoreCase)
                            ? EventFilterResponse.AcceptEvent
                            : EventFilterResponse.RejectProvider).Subscribe(this)
                );
        }
    }
}