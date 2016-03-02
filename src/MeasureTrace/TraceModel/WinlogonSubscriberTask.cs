// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using MeasureTrace.Calipers;

namespace MeasureTrace.TraceModel
{
    public class WinlogonSubscriberTask : MeasurementWithDuration, IMeasurement
    {
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }
        public int? SessionId { get; set; }
        public string TaskName { get; set; }
        public string SubscriberName { get; set; }

        public string SubscriberNameSummary
        {
            get
            {
                if (string.Equals(SubscriberName, "GPClient", StringComparison.OrdinalIgnoreCase))
                {
                    //  TODO FUTURE
                    //  this is a very crude mechanism for inferring sync versus async GP mode 
                    //  corresponding to the notification.
                    //  Figure out a way to build more formal relationship between WInlogon subscriber task and GP measurements
                    if (DurationMSec < 100)
                    {
                        return "GPClient (ForegroundAsync)";
                    }
                    return "GPClient (ForegroundSync)";
                }
                return SubscriberName;
            }
        }

        public int? ProcessId { get; set; }
        public WinlogonNotificationType NotificationType { get; set; }

        public string NotificationTypeLabel
        {
            get { return Enum.GetName(typeof (WinlogonNotificationType), NotificationType); }
        }

        //  TODO FUTURE
        //  Currently each measurement class has to implement MeasurementQuality, even when it is just a pass through as it is here.
        //  This is for compatibility with a bug in EF7
        //  Hopefully these can be removed in the future.
        public Trace Trace { get; set; }
        //public int TraceId { get; set; }
    }
}