using System;
using MeasureTrace.CalipersModel;

namespace MeasureTrace.Calipers
{
    public class TraceAttribute : ICaliper
    {
        public void RegisterFirstPass(TraceJob traceJob)
        {
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.NumberOfProcessors), e.NumberOfProcessors);
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU +=
                e => PublishSysConfigAttribute(traceJob, "CpuCount", e.NumberOfProcessors);
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.ComputerName), e.ComputerName);
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU +=
                e => PublishSysConfigAttribute(traceJob, "PrimaryDnsSuffix", e.DomainName);
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.MHz), e.MHz);
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.MemSize), e.MemSize);
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.PageSize), e.PageSize);
            traceJob.EtwTraceEventSource.Kernel.SysConfigBuildInfo +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.BuildLab), e.BuildLab);
            traceJob.EtwTraceEventSource.Kernel.SysConfigBuildInfo +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.ProductName), e.ProductName);
            traceJob.EtwTraceEventSource.Kernel.SysConfigBuildInfo +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.InstallDate), e.InstallDate);
            traceJob.EtwTraceEventSource.Kernel.SysConfigSystemPaths +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.SystemDirectory), e.SystemDirectory);
            traceJob.EtwTraceEventSource.Kernel.SysConfigSystemPaths +=
                e => PublishSysConfigAttribute(traceJob, nameof(e.SystemWindowsDirectory), e.SystemWindowsDirectory);
        }

        public void RegisterSecondPass(TraceJob traceJob)
        {
        }

        private static void PublishSysConfigAttribute(TraceJob traceJob, string propertyName, string stringValue)
        {
            traceJob.PublishMeasurement(
                new TraceModel.TraceAttribute
                {
                    Name = propertyName,
                    StringValue = stringValue
                }
                );
        }

        private static void PublishSysConfigAttribute(TraceJob traceJob, string propertyName, double doubleValue)
        {
            traceJob.PublishMeasurement(
                new TraceModel.TraceAttribute
                {
                    Name = propertyName,
                    DecimalValue = doubleValue
                }
                );
        }

        private static void PublishSysConfigAttribute(TraceJob traceJob, string propertyName, int intValue)
        {
            traceJob.PublishMeasurement(
                new TraceModel.TraceAttribute
                {
                    Name = propertyName,
                    WholeNumberValue = intValue
                }
                );
        }

        private static void PublishSysConfigAttribute(TraceJob traceJob, string propertyName, DateTime value)
        {
            traceJob.PublishMeasurement(
                new TraceModel.TraceAttribute
                {
                    Name = propertyName,
                    DateTimeValue = value
                }
                );
        }
    }
}