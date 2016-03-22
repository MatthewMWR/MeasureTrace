//// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

//using System;
//using MeasureTrace.Calipers;

//namespace MeasureTrace.TraceModel
//{
//    public class GroupPolicyActivity : MeasurementWithDuration, IMeasurement
//    {
//        public int Id { get; set; }
//        //public int MeasuredTraceId { get; set; }
//        public WinlogonNotificationType WinlogonNotificationType { get; set; }
//        public int ForSessionId { get; set; }
//        //public virtual WinlogonSubscriberTask WinlogonSubscriberTask { get; set; }

//        public Guid ActivityId { get; set; }
//        public PolicyApplicationMode Mode { get; set; }
//        public PolicyApplicationTrigger Trigger { get; set; }
//        public GpoScope Scope { get; set; }
//        public string PrincipalName { get; set; }
//        public string PrincipalDistinguishedName { get; set; }
//        public ReasonForSync ReasonForSync { get; set; }

//        public string ReasonForSyncLabel
//        {
//            get { return Enum.GetName(typeof (ReasonForSync), ReasonForSync); }
//        }

//        public string ScopeLabel
//        {
//            get { return Enum.GetName(typeof (GpoScope), Scope); }
//        }

//        public string TriggerLabel
//        {
//            get { return Enum.GetName(typeof (PolicyApplicationTrigger), Trigger); }
//        }

//        public string ModeLabel
//        {
//            get { return Enum.GetName(typeof (PolicyApplicationMode), Mode); }
//        }

//        //  TODO FUTURE
//        //  Currently each measurement class has to implement MeasurementQuality, even when it is just a pass through as it is here.
//        //  This is for compatibility with a bug in EF7
//        //  Hopefully these can be removed in the future.
//        public Trace Trace { get; set; }
//        //public int TraceId { get; set; }
//    }

//    public enum GpoScope
//    {
//        None = 0,
//        Machine,
//        User
//    }

//    public enum PolicyApplicationTrigger
//    {
//        None = 0,
//        Boot,
//        LogOn,
//        NetworkStateChange,
//        Manual,
//        Periodic,
//        Invalid
//    }

//    public enum PolicyApplicationMode
//    {
//        None = 0,
//        Background,
//        ForegroundSync,
//        ForegroundAsync
//    }
//}

