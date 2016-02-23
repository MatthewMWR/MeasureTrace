// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System.Collections.Generic;
using System.Linq;

namespace MeasureTrace.TraceModel
{
    public class PhysicalDisk : IMeasurement
    {
        public string ManufacturerLabel { get; set; }
        public string Model { get; set; }
        public int DiskNumber { get; set; }
        public string BootDriveLetter { get; set; }

        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }

        internal virtual ICollection<LogicalDisk> Volumes { get;
            //private set { DataValidation.IgnoreSetValue(value); }
        } = new List<LogicalDisk>();

        public bool ContainsWinDir
        {
            get { return Volumes.Count(v => v.ThisLogicalDiskContainsWinDir) > 0; }
        }

        public Trace Trace { get; set; }
        public int TraceId { get; set; }
        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;
    }
}