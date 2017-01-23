//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using MeasureTrace.Calipers;

namespace MeasureTrace.TraceModel
{
    public class WinlogonSubscriberTask : MeasurementWithDuration, IMeasurement
    {
#pragma warning disable 169
        // dummy "Backing field" for EF compat with no-setter properties
        private string _subscriberNameSummary;
#pragma warning restore 169
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
        public Trace Trace { get; set; }
    }
}