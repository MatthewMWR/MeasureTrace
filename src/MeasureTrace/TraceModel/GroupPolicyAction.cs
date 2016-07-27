// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Globalization;
using MeasureTrace.Calipers;
using Microsoft.Diagnostics.Tracing;

namespace MeasureTrace.TraceModel
{
    public class GroupPolicyAction : MeasurementWithDuration, IMeasurement
    {
        public GroupPolicyAction()
        {
        }

        public GroupPolicyAction(TraceEvent activityStartEvent)
        {
            ActivityId = activityStartEvent.ActivityID;
            Mode =
                GroupPolicyDomainKnowledge.MeasurePolicyApplicationMode(
                    (bool) activityStartEvent.PayloadByName("IsBackgroundProcessing"),
                    (bool) activityStartEvent.PayloadByName("IsAsyncProcessing"));
            Trigger =
                GroupPolicyDomainKnowledge.ResolvePolicyApplicationTrigger(
                    (int) activityStartEvent.ID);
            ReasonForSync =
                (ReasonForSync) (int) activityStartEvent.PayloadByName("ReasonForSyncProcessing");
            Scope =
                GroupPolicyDomainKnowledge.MeasureGpoScope(
                    (int) activityStartEvent.PayloadByName("IsMachine"));
        }

        public int Id { get; set; }
        public WinlogonNotificationType WinlogonNotificationType { get; set; }
        public GroupPolicyActionType ActionType { get; set; }
        public string ScriptName { get; set; }

        public string ActionLabel
        {
            get
            {
                if (ActionType == GroupPolicyActionType.RunCse)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}n_{1}__{2}", (int) ActionType,
                        Enum.GetName(typeof (GroupPolicyActionType), ActionType), CseLabel);
                }
                if (ActionType == GroupPolicyActionType.RunScript)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}n_{1}__{2}", (int) ActionType,
                        Enum.GetName(typeof (GroupPolicyActionType), ActionType), ScriptName);
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}__{1}", (int) ActionType,
                    Enum.GetName(typeof (GroupPolicyActionType), ActionType));
            }
        }
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private string _actionLabel;
#pragma warning restore 169

        public int ForSessionId { get; set; }

        public Guid ActivityId { get; set; }
        public PolicyApplicationMode Mode { get; set; }
        public PolicyApplicationTrigger Trigger { get; set; }
        public GpoScope Scope { get; set; }
        public string PrincipalName { get; set; }
        public string PrincipalDistinguishedName { get; set; }
        public Guid? CseGuid { get; set; }

        public string CseLabel
        {
            get { return CseGuid == null ? null : GroupPolicyDomainKnowledge.GetCseLabelInvariant(CseGuid.Value); }
        }
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private string _cseLabel;
#pragma warning restore 169

        public ReasonForSync ReasonForSync { get; set; }

        public Trace Trace { get; set; }
    }

    public enum GroupPolicyActionType
    {
        None = 0,
        WaitForConnect = 1,
        FindAndFilterGpos,
        RunCse,
        CompleteSession,
        RunScript
    }
}