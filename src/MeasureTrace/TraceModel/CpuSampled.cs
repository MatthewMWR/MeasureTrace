//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public class CpuSampled : IMeasurement
    {
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private bool _isConsumingAtLeastOneCore;
#pragma warning restore 169
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private string _source;
#pragma warning restore 169
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private string _sourceName;
#pragma warning restore 169
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private double _weight;
#pragma warning restore 169
        public int Id { get; set; }
        public int Count { get; set; }
        public int TotalSamplesDuringInterval { get; set; }
        public double Weight => (double) Count/(double) TotalSamplesDuringInterval;
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public int ThreadId { get; set; }
        public bool IsDpc { get; set; }
        public bool IsIsr { get; set; }
        public string Source => CalculateSource(IsDpc, IsIsr, ProcessName, ProcessId);
        public string SourceName => CalculateSource(IsDpc, IsIsr, ProcessName);
        public int CpuCoreCount { get; set; }

        public bool IsComsumingAtLeastOneCore
        {
            get
            {
                if (CpuCoreCount < 1) return false;
                var samplesPerCore = TotalSamplesDuringInterval/CpuCoreCount;
                var threshold = samplesPerCore*0.95;
                return Count > threshold;
            }
        }

        public int TimeSliceIndex { get; set; }
        public int TimeSliceLengthMSec { get; set; }
        public Trace Trace { get; set; }
        //public int TraceId { get; set; }
        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;

        public static string CalculateSource(bool isDpc, bool isIsr, string processName, int processId)
        {
            return isDpc ? "DPC" : isIsr ? "ISR" : $"{processName} ({processId})";
        }

        public static string CalculateSource(bool isDpc, bool isIsr, string processName)
        {
            return isDpc ? "DPC" : isIsr ? "ISR" : $"{processName}";
        }
    }
}