// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace MeasureTrace.Calipers
{
    public class CpuSampled : ICaliper
    {
        private readonly int _intervalLengthMSec = 5000;
        private readonly List<TraceModel.CpuSampled> _rawMeasurementsForCurrentTimeSlice = new List<TraceModel.CpuSampled>();


        private int _cpuCoreCount;
        private int _currentIntervalIndex;
        private double _nextTimeSliceStartOffsetMSec;
        private TraceJob _traceJob;

        public Action Aggregator;

        public void RegisterFirstPass(TraceJob traceJob)
        {
            _traceJob = traceJob;
            traceJob.EtwTraceEventSource.Kernel.SystemConfigCPU += data => _cpuCoreCount = data.NumberOfProcessors;
        }

        public void RegisterSecondPass(TraceJob traceJob)
        {
            traceJob.EtwTraceEventSource.Kernel.PerfInfoSample += OnCpuSample;
            traceJob.EtwTraceEventSource.Completed += RolloverTimeSliceOnCompletion;
        }

        private void RolloverTimeSliceOnCompletion()
        {
            RolloverTimeSliceAsNeeded(double.MaxValue);
        }

        //public Func Grouper;

        private void OnCpuSample(SampledProfileTraceData sample)
        {
            RolloverTimeSliceAsNeeded(sample.TimeStampRelativeMSec);

            var cpuSampled = new TraceModel.CpuSampled
            {
                Count = sample.Count,
                CpuCoreCount = _cpuCoreCount,
                IsDpc = sample.ExecutingDPC,
                IsIsr = sample.ExecutingISR,
                ProcessId = sample.ProcessID,
                ProcessName = sample.ProcessName,
                ThreadId = sample.ThreadID
            };

            _rawMeasurementsForCurrentTimeSlice.Add(cpuSampled);
        }

        private void RolloverTimeSliceAsNeeded(double timestampRelativeMSec)
        {
            if (timestampRelativeMSec < _nextTimeSliceStartOffsetMSec) return;
            _traceJob.PublishMeasurement(
                _rawMeasurementsForCurrentTimeSlice.GroupBy(cs => cs.Source)
                    .Select(g => new TraceModel.CpuSampled
                    {
                        Count = g.Sum(cs => cs.Count),
                        CpuCoreCount = _cpuCoreCount,
                        IsIsr = g.First().IsIsr,
                        IsDpc = g.First().IsDpc,
                        ProcessId = g.First().ProcessId,
                        ProcessName = g.First().ProcessName,
                        ThreadId = -1,
                        TimeSliceLengthMSec = _intervalLengthMSec,
                        TimeSliceIndex = _currentIntervalIndex,
                        TotalSamplesDuringInterval = _rawMeasurementsForCurrentTimeSlice.Count
                    })
                );
            _rawMeasurementsForCurrentTimeSlice.Clear();
            _currentIntervalIndex++;
            _nextTimeSliceStartOffsetMSec = _currentIntervalIndex*_intervalLengthMSec;
        }
    }
}