//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Generic;
using System.Linq;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace MeasureTrace.Calipers
{
    public class DiskProcessor : ProcessorBase, IObserver<SystemConfigPhyDiskTraceData>,
        IObserver<SystemConfigLogDiskTraceData>, IObserver<SystemPathsTraceData>
    {
        private readonly ICollection<LogicalDisk> _logicalDisksPartial = new List<LogicalDisk>();
        private readonly ICollection<PhysicalDisk> _physicalDisksPartial = new List<PhysicalDisk>();
        private readonly ICollection<SystemPathsTraceData> _sysPaths = new List<SystemPathsTraceData>();
        private int _onCompletedCallCount;

        public void OnNext(SystemConfigLogDiskTraceData lDiskEvent)
        {
            if (lDiskEvent == null) throw new ArgumentNullException("lDiskEvent");
            var logicalDiskMeasurement = new LogicalDisk
            {
                DiskNumber = lDiskEvent.DiskNumber,
                LogicalDriveLetter = lDiskEvent.DriveLetterString,
                DriveType = lDiskEvent.DriveType,
                FileSystem = lDiskEvent.FileSystem,
                PartitionSize = lDiskEvent.PartitionSize,
                Size = lDiskEvent.Size,
                PartitionNumber = lDiskEvent.PartitionNumber
            };
            _logicalDisksPartial.Add(logicalDiskMeasurement);
        }

        public void OnNext(SystemConfigPhyDiskTraceData value)
        {
            if (value == null) throw new ArgumentNullException("value");
            var physicalDiskMeasurement = new PhysicalDisk
            {
                BootDriveLetter = value.BootDriveLetter,
                DiskNumber = value.DiskNumber,
                ManufacturerLabel = value.Manufacturer
            };
            _physicalDisksPartial.Add(physicalDiskMeasurement);
        }

        public void OnCompleted()
        {
            //  TODO FUTURE 
            //  try to move the decorating and publishing into OnNext flow so dependent parties don't
            //  have to wait until the end.
            //  This is tricky, though, with partial information
            //  In any case these sysconfig events are usually at the end of the trace anyway
            //  To really free up this reference info for other callers may have to move 
            //  to explicit two pass model

            //  Don't do anything unless it is the last call
            _onCompletedCallCount++;
            if (_onCompletedCallCount < Subscriptions.Count) return;


            //  Decorate extra volume properties and publish
            var sysPathsEntryInUse = _sysPaths.FirstOrDefault();
            foreach (var lDisk in _logicalDisksPartial)
            {
                lDisk.SystemWideWinDirPath = sysPathsEntryInUse == null
                    ? string.Empty
                    : sysPathsEntryInUse.SystemWindowsDirectory;

                lDisk.SystemWideWinDirSystem32Path = sysPathsEntryInUse == null
                    ? string.Empty
                    : sysPathsEntryInUse.SystemDirectory;
                RegisterMeasurement(lDisk);
            }

            //  Decorate physical disk properties and publish
            foreach (var pDisk in _physicalDisksPartial)
            {
                var disk = pDisk;
                foreach (var lDisk in _logicalDisksPartial.Where(ld => ld.DiskNumber == disk.DiskNumber))
                {
                    pDisk.Volumes.Add(lDisk);
                    RegisterMeasurement(pDisk);
                }
            }
        }

        public void OnNext(SystemPathsTraceData sysPathEvent)
        {
            _sysPaths.Add(sysPathEvent);
        }

        public override void Initialize(TraceJob traceJob)
        {
            Subscriptions.Add(
                traceJob.EtwTraceEventSource.Kernel.Observe<SystemConfigPhyDiskTraceData>().Subscribe(this)
                );

            Subscriptions.Add(
                traceJob.EtwTraceEventSource.Kernel.Observe<SystemConfigLogDiskTraceData>().Subscribe(this)
                );

            Subscriptions.Add(
                traceJob.EtwTraceEventSource.Kernel.Observe<SystemPathsTraceData>().Subscribe(this)
                );
        }
    }
}