//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System.Collections.Generic;
using System.Linq;

namespace MeasureTrace.TraceModel
{
    public class PhysicalDisk : IMeasurement
    {
#pragma warning disable 169
        // dummy "backing field" for compat with EF7
        private bool _containsWinDir;
#pragma warning restore 169
        public string ManufacturerLabel { get; set; }
        public string Model { get; set; }
        public int DiskNumber { get; set; }
        public string BootDriveLetter { get; set; }
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }

        internal virtual ICollection<LogicalDisk> Volumes { get; } = new List<LogicalDisk>();

        public bool ContainsWinDir
        {
            get { return Volumes.Count(v => v.ThisLogicalDiskContainsWinDir) > 0; }
        }

        public Trace Trace { get; set; }
        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;
    }
}