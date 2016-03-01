// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using MeasureTrace.Adapters;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;

#pragma warning disable 618

namespace MeasureTrace.Calipers
{
    public class GroupPolicyActionProcessor : ProcessorBase, IObserver<IMeasurement>, IObserver<TraceEvent>
    {
        private readonly List<IsConsumedDecorator<TraceEvent>> _gpEvents = new List<IsConsumedDecorator<TraceEvent>>();

        public void OnNext(IMeasurement value)
        {
            var wlMeasurement = (WinlogonSubscriberTask) value;
            if (wlMeasurement != null)
            {
                AssembleGpActionsAsPossible();
            }
        }

        public void OnCompleted()
        {
            AssembleGpActionsAsPossible();
        }

        public void OnNext(TraceEvent value)
        {
            _gpEvents.Add(new IsConsumedDecorator<TraceEvent>(value));
            AssembleGpActionsAsPossible();
        }

        public override void Initialize(TraceJob traceJob)
        {
            if (traceJob == null) throw new ArgumentNullException(nameof(traceJob));

            Subscriptions.Add(
                traceJob.EtwTraceEventSource.Registered.Observe(GroupPolicyDomainKnowledge.GroupPolicyProviderName, null)
                    .Subscribe(this)
                );

            Subscriptions.Add(
                traceJob.RegisterProcessorByType<WinlogonSubscriberProcessor>(
                    ProcessorTypeCollisionOption.UseExistingIfFound).Subscribe(this)
                );
        }

        private void AssembleGpActionsAsPossible()
        {
            foreach (var activityIdGroup in _gpEvents.GroupBy(x => x.Value.ActivityID))
            {
                AssembleScriptRunsAsPossible(activityIdGroup.AsEnumerable());
                var activityStartEvent =
                    activityIdGroup.AsEnumerable()
                        .FirstOrDefault(e => GroupPolicyDomainKnowledge.IsActivityStartEventId((int) e.Value.ID));
                if (activityStartEvent == null) continue;
                AssembleFirstPhasesAsPossible(activityStartEvent.Value, activityIdGroup.AsEnumerable());
                AssembleSecondPhasesAsPossible(activityStartEvent.Value, activityIdGroup.AsEnumerable());
                AssembleCseActionsAsPossible(activityStartEvent.Value, activityIdGroup.AsEnumerable());
            }
        }

        private void AssembleScriptRunsAsPossible(IEnumerable<IsConsumedDecorator<TraceEvent>> activityEvents)
        {
            foreach (
                var tidGroup in
                    activityEvents.Where(
                        e =>
                            e.IsConsumed == false &&
                            ((int) e.Value.ID == GroupPolicyDomainKnowledge.ScriptStartEventId ||
                             (int) e.Value.ID == GroupPolicyDomainKnowledge.ScriptStopEventId))
                        .GroupBy(e => e.Value.ThreadID))
            {
                var start =
                    tidGroup.FirstOrDefault(e => (int) e.Value.ID == GroupPolicyDomainKnowledge.ScriptStartEventId);
                var stop =
                    tidGroup.FirstOrDefault(e => (int) e.Value.ID == GroupPolicyDomainKnowledge.ScriptStopEventId);
                if (start == null || stop == null) continue;
                var m = new GroupPolicyAction
                {
                    ActionType = GroupPolicyActionType.RunScript,
                    Scope = (int) start.Value.PayloadByName("ScriptType") == 1 ? GpoScope.User : GpoScope.Machine,
                    DurationMSec = stop.Value.TimeStampRelativeMSec - start.Value.TimeStampRelativeMSec,
                    Mode =
                        (bool) start.Value.PayloadByName("IsScriptSync")
                            ? PolicyApplicationMode.ForegroundSync
                            : PolicyApplicationMode.ForegroundAsync,
                    Trigger =
                        (int) start.Value.PayloadByName("ScriptType") == 1
                            ? PolicyApplicationTrigger.LogOn
                            : PolicyApplicationTrigger.Boot
                };
                start.IsConsumed = true;
                stop.IsConsumed = true;
                RegisterMeasurement(m);
            }
        }

        private void AssembleSecondPhasesAsPossible(TraceEvent activityStartEvent,
            IEnumerable<IsConsumedDecorator<TraceEvent>> activityEvents)
        {
            var firstPhaseEnd = activityEvents.FirstOrDefault(e => (int) e.Value.ID == 5311);
            if (firstPhaseEnd == null) return;
            var secondPhaseEnd = activityEvents.FirstOrDefault(e => (int) e.Value.ID == 5312 && !e.IsConsumed);
            if (secondPhaseEnd == null) return;

            var m = new GroupPolicyAction(activityStartEvent)
            {
                DurationMSec = secondPhaseEnd.Value.TimeStampRelativeMSec - firstPhaseEnd.Value.TimeStampRelativeMSec,
                ActionType = GroupPolicyActionType.FindAndFilterGpos
            };

            RegisterMeasurement(m);
            secondPhaseEnd.IsConsumed = true;
        }

        private void AssembleFirstPhasesAsPossible(TraceEvent activityStartEvent,
            IEnumerable<IsConsumedDecorator<TraceEvent>> activityEvents)
        {
            var firstPhaseEnd = activityEvents.FirstOrDefault(e => (int) e.Value.ID == 5311 && !e.IsConsumed);
            if (firstPhaseEnd == null) return;

            var m = new GroupPolicyAction(activityStartEvent)
            {
                DurationMSec = firstPhaseEnd.Value.TimeStampRelativeMSec - activityStartEvent.TimeStampRelativeMSec,
                ActionType = GroupPolicyActionType.WaitForConnect
            };

            RegisterMeasurement(m);
            firstPhaseEnd.IsConsumed = true;
        }

        private void AssembleCseActionsAsPossible(TraceEvent activityStartEvent,
            IEnumerable<IsConsumedDecorator<TraceEvent>> activityEvents)
        {
            foreach (
                var cseStopEvent in
                    activityEvents.Where(
                        et => GroupPolicyDomainKnowledge.IsCseEndEventId((int) et.Value.ID) && et.IsConsumed == false))
            {
                var cseId = (Guid) cseStopEvent.Value.PayloadByName(GroupPolicyDomainKnowledge.CseIdFieldName);
                var csePurportedDurationMSec =
                    (int) cseStopEvent.Value.PayloadByName(GroupPolicyDomainKnowledge.CseDurationFieldName);

                var m = new GroupPolicyAction(activityStartEvent)
                {
                    DurationMSec = csePurportedDurationMSec,
                    CseGuid = cseId,
                    ActionType = GroupPolicyActionType.RunCse
                };
                cseStopEvent.IsConsumed = true;
                RegisterMeasurement(m);
            }
        }

        private class IsConsumedDecorator<T>
        {
            private readonly object _decoratedObject;

            internal IsConsumedDecorator(object decoratedObject)
            {
                _decoratedObject = decoratedObject;
            }

            internal bool IsConsumed { get; set; }
            internal T Value => (T) _decoratedObject;
        }
    }
}