//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

#pragma warning disable 618

namespace MeasureTrace.Calipers
{
    public sealed class ServiceTransitionProcessor : ProcessorBase, IObserver<TraceEvent>, IObserver<IMeasurement>
    {
        private const string ServicesProviderName = "Microsoft-Windows-Services";

        private readonly ICollection<ServiceTransition> _serviceTransitionsAwaitingSysConfigDetails =
            new List<ServiceTransition>();

        private readonly ICollection<SystemConfigServicesTraceData> _sysConfigServicesEvents =
            new List<SystemConfigServicesTraceData>();

        private readonly Dictionary<string, TraceEvent> _transitionBeginEvents = new Dictionary<string, TraceEvent>();
        //private bool _abort;

        public void OnNext(IMeasurement value)
        {
            //var sleeper = value as SystemSleep;
            //if (sleeper != null) _abort = true;
        }

        public void OnNext(TraceEvent traceEvent)
        {
            //if (_abort) return;
            //  Process core event
            if ((int) traceEvent.ID == 105)
            {
                TriageEvent(traceEvent);
            }

            //  Also try reconcile any pending measurements lacking properties
            if (_sysConfigServicesEvents.Count > 0)
            {
                var removalQueue = new List<ServiceTransition>();
                foreach (var measurement in _serviceTransitionsAwaitingSysConfigDetails)
                {
                    if (DecorateServiceTransition(measurement, _sysConfigServicesEvents))
                    {
                        removalQueue.Add(measurement);
                        RegisterMeasurement(measurement);
                    }
                }
                foreach (var measurement in removalQueue)
                {
                    _serviceTransitionsAwaitingSysConfigDetails.Remove(measurement);
                }
            }
        }

        public void OnCompleted()
        {
            //if (_abort) return;
            //  Process remaining data
            foreach (var measurement in _serviceTransitionsAwaitingSysConfigDetails)
            {
                DecorateServiceTransition(measurement, _sysConfigServicesEvents);
                RegisterMeasurement(measurement);
            }
        }

        public override void Initialize(TraceJob traceJob)
        {
            var observable = traceJob.EtwTraceEventSource.Registered.Observe(
                (pn, en) =>
                    string.Compare(ServicesProviderName, pn, StringComparison.OrdinalIgnoreCase) == 0
                        ? EventFilterResponse.AcceptEvent
                        : EventFilterResponse.RejectProvider);
            Subscriptions.Add(observable.Subscribe(this));

            //traceJob.RegisterProcessorInstance(new SystemSleepProcessor());
            //Subscriptions.Add(
            //    traceJob.GetRegisteredProcessors().OfType<SystemSleepProcessor>().Select(x => x.Subscribe(this)).First()
            //    );
            traceJob.EtwTraceEventSource.Kernel.SystemConfigServices +=
                e => _sysConfigServicesEvents.Add((SystemConfigServicesTraceData) e.Clone());
        }

        private static ServiceTransition MakeServiceTransition(TraceEvent startEvent,
            TraceEvent stopEvent,
            ServiceTransitionTypeEx transitionType)
        {
            var imageName = (string) stopEvent.PayloadByName("ImageName");
            return new ServiceTransition
            {
                ServiceName = startEvent.PayloadString(4),
                ServiceTransitionType = transitionType,
                DurationMSec = stopEvent.TimeStampRelativeMSec - startEvent.TimeStampRelativeMSec,
                ServiceProcessName = imageName,
                ExecutionPhase = (ServiceExecutionPhase) (int) startEvent.PayloadValue(0)
            };
        }

        private static bool DecorateServiceTransition(ServiceTransition serviceTransition,
            IEnumerable<SystemConfigServicesTraceData> sysConfigServicesEvents)
        {
            var matchingSysConfig =
                sysConfigServicesEvents.FirstOrDefault(
                    e =>
                        string.Compare(e.ServiceName, serviceTransition.ServiceName,
                            StringComparison.OrdinalIgnoreCase) == 0);
            if (matchingSysConfig == null)
            {
                return false;
            }
            //serviceTransition.DisplayName = matchingSysConfig.DisplayName;
            return true;
        }

        private void TriageEvent(TraceEvent traceEvent)
        {
            var serviceName = (string) traceEvent.PayloadValue(4);
            if (!_transitionBeginEvents.ContainsKey(serviceName))
            {
                _transitionBeginEvents.Add(serviceName, traceEvent);
            }
            else
            {
                var oldEvent = _transitionBeginEvents[serviceName];
                var oldStatus = (ServiceControllerStatus) (int) _transitionBeginEvents[serviceName].PayloadValue(1);
                var newStatus = (ServiceControllerStatus) (int) traceEvent.PayloadValue(1);
                var transition = ServicesDomainKnowledge.MeasureServiceTranitionStatus(oldStatus, newStatus);
                if (transition == ServiceTransitionTypeEx.None) return;
                _transitionBeginEvents[serviceName] = traceEvent;
                var measurement = MakeServiceTransition(oldEvent, traceEvent, transition);
                if (DecorateServiceTransition(measurement, _sysConfigServicesEvents))
                {
                    RegisterMeasurement(measurement);
                }
                else
                {
                    _serviceTransitionsAwaitingSysConfigDetails.Add(measurement);
                }
            }
        }
    }
}