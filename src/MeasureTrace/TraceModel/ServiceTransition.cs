﻿//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public class ServiceTransition : MeasurementWithDuration, IMeasurement
    {
#pragma warning disable 169
        // dummy "Backing field" for EF compat with no-setter properties
        private double _serviceNameSummary;
#pragma warning restore 169
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }

        public string ServiceNameSummary
        {
            get
            {
                if (ServiceName.Contains(@"$"))
                {
                    return ServiceName.Split('$')[0];
                }
                return ServiceName;
            }
        }

        public ServiceExecutionPhase ExecutionPhase { get; set; }
        public string ServiceProcessName { get; set; }
        public ServiceStartupType ConfiguredStartType { get; set; }
        public bool? HasTriggers { get; set; }
        public ServiceTransitionTypeEx ServiceTransitionType { get; set; }
        public bool IsAutoStartPhase { get; set; }
        public string LoadOrderGroup { get; set; }
        //  TODO FUTURE
        //  Currently each measurement class has to implement MeasurementQuality, even when it is just a pass through as it is here.
        //  This is for compatibility with a bug in EF7
        //  Hopefully these can be removed in the future.
        public Trace Trace { get; set; }
    }
}