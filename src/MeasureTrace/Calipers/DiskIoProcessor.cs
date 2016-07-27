// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using MeasureTrace.Adapters;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;

namespace MeasureTrace.Calipers
{
    public class DiskIoProcessor : ProcessorBase
    {
        private readonly Dictionary<string, DiskIo> _aProcessAndFile =
            new Dictionary<string, DiskIo>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, string> _diskIoTempFiles = new Dictionary<string, string>();

        public DiskIoProcessor()
        {
            AggregateByProcessAndFile = true;
            ReducePathEntropy = true;
            RegisterNonAggregatedMeasurements = false;
        }

        public bool AggregateByProcessAndFile { get; set; }
        public bool ReducePathEntropy { get; set; }
        public bool RegisterNonAggregatedMeasurements { get; set; }

        public override void Initialize(TraceJob traceJob)
        {
            if (!WptInterop.IsXperfInstalled())
            {
                Logging.LogDebugMessage("xperf not found. skipping disk IO processing");
                return;
            }
            _diskIoTempFiles = WptInterop.RunXPerfAllProcessing(traceJob.EtwTraceEventSource.LogFileName);
            PreTraceEventProcessing += ProcessDiskIoLog;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Just want to retry deleting temp file before giving up, don't care what the exception is")]
        private void ProcessDiskIoLog()
        {
            var entropyOptions = new ReducePathEntropyOptions();
            var replacements = DiskIoDomainKnowledge.NewDefaultReplacementsTable();
            var specificTempFile = new FileInfo(_diskIoTempFiles[WptInterop.DiskIoTempOutputLabel]);
            foreach (
                var m in
                    WptInterop.GetDiskIoMeasurementsFromXPerfOutput(specificTempFile.FullName))
            {
                m.PathSummary = ReducePathEntropy
                    ? DiskIoDomainKnowledge.ReducePathEntropy(m.PathRaw, replacements, entropyOptions)
                    : m.PathRaw;
                m.AttributionBucket = DiskIoDomainKnowledge.ResolveDiskIoAttributionBucket(m.ProcessName, m.PathRaw,
                    m.PathSummary);
                if (RegisterNonAggregatedMeasurements) RegisterMeasurement(m);
                if (AggregateByProcessAndFile)
                {
                    if (!_aProcessAndFile.ContainsKey(m.ProcessAndPathSummaryKey))
                    {
                        var m2 = (DiskIo) m.Clone();
                        m2.IsAggregate = true;
                        m2.IoType = "MixedAggregate";
                        _aProcessAndFile.Add(m.ProcessAndPathSummaryKey, m2);
                    }
                    else
                    {
                        var updateThis = _aProcessAndFile[m.ProcessAndPathSummaryKey];
                        updateThis.IoTimeUSec += m.IoTimeUSec;
                        updateThis.DiskSvcTimeUSec += m.DiskSvcTimeUSec;
                        updateThis.Count++;
                        updateThis.Bytes += m.Bytes;
                    }
                }
            }
            var i = 0;
            while (i < 4)
            {
                i++;
                try
                {
                    File.Delete(_diskIoTempFiles[WptInterop.DiskIoTempOutputLabel]);
                    File.Delete(_diskIoTempFiles[WptInterop.DiskIoTempErrorLabel]);
                    break;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
            if (!AggregateByProcessAndFile) return;
            foreach (var am in _aProcessAndFile.Values)
            {
                RegisterMeasurement(am);
            }
        }
    }
}