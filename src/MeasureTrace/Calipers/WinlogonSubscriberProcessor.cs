//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 

//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using MeasureTrace.Adapters;
//using MeasureTrace.CalipersModel;
//using MeasureTrace.TraceModel;
//using Microsoft.Diagnostics.Tracing;

//#pragma warning disable 618

//namespace MeasureTrace.Calipers
//{
//    /// <summary>
//    ///     Measures occurence and durations of Winlogon subscribers notifications based on Winlogon events
//    ///     Similar to WPA FullBoot profile logic, but with more session awareness so as not to get confused by
//    ///     RDP logons and such.
//    /// </summary>
//    public class WinlogonSubscriberProcessor : ProcessorBase, IObserver<TraceEvent>, IObserver<IMeasurement>
//    {
//        private const string WinlogonProviderName = "Microsoft-Windows-Winlogon";
//        private readonly ICollection<TerminalSession> _knownSessions = new List<TerminalSession>();
//        private readonly ICollection<TraceEvent> _outstandingNotifyEvents = new List<TraceEvent>();
//        private readonly ICollection<WinlogonSubscriberTask> _partialTasks = new List<WinlogonSubscriberTask>();

//        public void OnNext(IMeasurement value)
//        {
//            _knownSessions.Add((TerminalSession) value);
//        }

//        public void OnNext(TraceEvent traceEvent)
//        {
//            if ((int) traceEvent.ID == 805)
//            {
//                _outstandingNotifyEvents.Add(traceEvent);
//            }
//            if ((int) traceEvent.ID == 806)
//            {
//                var notifyStartEvent =
//                    _outstandingNotifyEvents.FirstOrDefault(
//                        e => e.ProcessID == traceEvent.ProcessID
//                             &&
//                             string.Compare((string) e.PayloadByName("SubscriberName"),
//                                 (string) traceEvent.PayloadByName("SubscriberName"), StringComparison.OrdinalIgnoreCase) ==
//                             0
//                             && string.Compare(e.TaskName, traceEvent.TaskName, StringComparison.OrdinalIgnoreCase) == 0
//                             && (int) e.PayloadValue(0) == (int) traceEvent.PayloadValue(0)
//                        );
//                if (notifyStartEvent == null)
//                {
//                    //  TODO FUTURE decide if there is anything to do with orphans
//                    return;
//                }

//                var wlTask = new WinlogonSubscriberTask
//                {
//                    DurationMSec = traceEvent.TimeStampRelativeMSec - notifyStartEvent.TimeStampRelativeMSec,
//                    SubscriberName = (string) traceEvent.PayloadByName("SubscriberName"),
//                    TaskName = traceEvent.TaskName,
//                    ProcessId = traceEvent.ProcessID,
//                    NotificationType = (WinlogonNotificationType) (int) traceEvent.PayloadValue(0)
//                };


//                _partialTasks.Add(wlTask);

//                if (_knownSessions.Count > 0)
//                {
//                    DecorateAndRegisterTasks(_partialTasks, _knownSessions);
//                }
//            }
//        }

//        public void OnCompleted()
//        {
//            DecorateAndRegisterTasks(_partialTasks, _knownSessions);

//            // Any remaining winlogon tasks we don't have session context for, so register them as is
//            foreach (var t in _partialTasks)
//            {
//                RegisterMeasurement(t);
//            }
//        }

//        //  TraceProcessJob is responsible for the lifecycle of the processor after RegisterProcessorInstance()
//        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
//        public override void Initialize(TraceJob traceProcess)
//        {
//            Subscriptions.Add(
//                traceProcess.RegisterProcessorByType<TerminalSessionProcessor>(
//                    ProcessorTypeCollisionOption.UseExistingIfFound).Subscribe(this));

//            var winlogonObserve = traceProcess.EtwTraceEventSource.Registered.Observe(
//                (p, e) =>
//                    string.Compare(p, WinlogonProviderName, StringComparison.OrdinalIgnoreCase) == 0
//                        ? EventFilterResponse.AcceptEvent
//                        : EventFilterResponse.RejectProvider);

//            Subscriptions.Add(winlogonObserve.Subscribe(this));
//        }

//        private void DecorateAndRegisterTasks(ICollection<WinlogonSubscriberTask> tasks,
//            ICollection<TerminalSession> knownSessions)
//        {
//            var completed = new List<WinlogonSubscriberTask>();
//            foreach (var task in tasks)
//            {
//                var matchingSession = knownSessions.FirstOrDefault(s => s.WinlogonPid == task.ProcessId);
//                if (matchingSession == null) continue;
//                task.SessionId = matchingSession.SessionId;
//                RegisterMeasurement(task);
//                completed.Add(task);
//            }

//            foreach (var comp in completed)
//            {
//                tasks.Remove(comp);
//            }
//        }
//    }
//}