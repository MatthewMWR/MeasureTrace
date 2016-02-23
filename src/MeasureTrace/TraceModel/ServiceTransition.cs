// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;

namespace MeasureTrace.TraceModel
{
    public class ServiceTransition : MeasurementWithDuration, IMeasurement
    {
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }

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

        public string ExcutionPhaseLabel
        {
            get { return Enum.GetName(typeof (ServiceExecutionPhase), ExecutionPhase); }
        }

        public string ServiceProcessName { get; set; }
        public ServiceStartupType ConfiguredStartType { get; set; }

        public string ConfiguredStartTypeLabel
        {
            get { return Enum.GetName(typeof (ServiceStartupType), ConfiguredStartType); }
        }

        public bool? HasTriggers { get; set; }
        public ServiceTransitionTypeEx ServiceTransitionType { get; set; }

        public string ServiceTransitionTypeLabel
        {
            get { return Enum.GetName(typeof (ServiceTransitionTypeEx), ServiceTransitionType); }
        }

        public bool IsAutoStartPhase { get; set; }
        public string LoadOrderGroup { get; set; }
        //  TODO FUTURE
        //  Currently each measurement class has to implement MeasurementQuality, even when it is just a pass through as it is here.
        //  This is for compatibility with a bug in EF7
        //  Hopefully these can be removed in the future.
        public Trace Trace { get; set; }
        public int TraceId { get; set; }
    }
}