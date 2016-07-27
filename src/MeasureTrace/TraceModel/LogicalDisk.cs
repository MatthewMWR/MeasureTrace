// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;

namespace MeasureTrace.TraceModel
{
    public class LogicalDisk : IMeasurement
    {
        public int DiskNumber { get; set; }
        public string LogicalDriveLetter { get; set; }
        public int DriveType { get; set; }
        public string FileSystem { get; set; }
        public int PartitionNumber { get; set; }
        public long PartitionSize { get; set; }
        public int Size { get; set; }
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }

        public bool ThisLogicalDiskContainsWinDir
        {
            get
            {
                return !string.IsNullOrWhiteSpace(SystemWideWinDirPath) &&
                       SystemWideWinDirPath.StartsWith(LogicalDriveLetter,
                           StringComparison.OrdinalIgnoreCase);
            }
        }
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private bool _thisLogicalDiskContainsWinDir;
#pragma warning restore 169

        public string SystemWideWinDirPath { get; set; }
        public string SystemWideWinDirSystem32Path { get; set; }
        public Trace Trace { get; set; }
        //public int TraceId { get; set; }
        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;
    }
}