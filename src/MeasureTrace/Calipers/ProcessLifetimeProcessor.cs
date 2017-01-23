//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Concurrent;
using System.Threading;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace MeasureTrace.Calipers
{
    public class ProcessLifetimeProcessor : ProcessorBase, IObserver<ProcessTraceData>
    {
        private readonly ConcurrentDictionary<ulong, ProcessTraceData> _outstandingProcessEvents =
            new ConcurrentDictionary<ulong, ProcessTraceData>();

        private TraceJob _traceJob;

        public void OnNext(ProcessTraceData value)
        {
            if (string.Compare(value.OpcodeName, "Start", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(value.OpcodeName, "DCStart", StringComparison.OrdinalIgnoreCase) == 0
                )
            {
                _outstandingProcessEvents.GetOrAdd(value.UniqueProcessKey, value);
            }
            else if (string.Compare(value.OpcodeName, "Stop", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(value.OpcodeName, "DCStop", StringComparison.OrdinalIgnoreCase) == 0
                )
            {
                //  This is a process stop event, so we need to make a ProcessLifeTime
                ProcessTraceData correspondingStartEvent = null;
                while (_outstandingProcessEvents.ContainsKey(value.UniqueProcessKey) &&
                       !_outstandingProcessEvents.TryRemove(value.UniqueProcessKey, out correspondingStartEvent))
                {
                    Thread.Sleep(100);
                }

                //var startOffset = correspondingStartEvent == null ? -1 : correspondingStartEvent.TimeStampRelativeMSec;

                var duration = correspondingStartEvent == null
                    ? TimeSpan.FromMilliseconds(value.TimeStampRelativeMSec)
                    : TimeSpan.FromMilliseconds(value.TimeStampRelativeMSec -
                                                correspondingStartEvent.TimeStampRelativeMSec);

                RegisterMeasurement(
                    new ProcessLifetime
                    {
                        DurationMSec = duration.TotalMilliseconds,
                        ImageName = value.ImageFileName,
                        SessionId = value.SessionID
                    }
                    );
            }
            else
            {
                Console.WriteLine(value.OpcodeName);
            }
        }

        public void OnCompleted()
        {
            //  Any remaining process start events will be processes that did not end, so represent these as well
            foreach (var startEvent in _outstandingProcessEvents.Values)
            {
                RegisterMeasurement(
                    new ProcessLifetime
                    {
                        DurationMSec =
                            _traceJob.EtwTraceEventSource.SessionDuration.TotalMilliseconds -
                            startEvent.TimeStampRelativeMSec,
                        ImageName = startEvent.ImageFileName,
                        SessionId = startEvent.SessionID
                    }
                    );
            }
        }

        public override void Initialize(TraceJob traceJob)
        {
            var observable = traceJob.EtwTraceEventSource.Kernel.Observe<ProcessTraceData>();
            Subscriptions.Add(observable.Subscribe(this));
            _traceJob = traceJob;
        }
    }
}