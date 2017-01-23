//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.IO;
using System.Linq;
using MeasureTrace;
using MeasureTrace.Adapters;
using MeasureTrace.Calipers;
using MeasureTrace.TraceModel;
using Xunit;
using BootPhase = MeasureTrace.Calipers.BootPhase;
using CpuSampled = MeasureTrace.Calipers.CpuSampled;

namespace MeasureTraceTests
{
    public class SmokeTests
    {
        [Theory]
        [InlineData(@"TestData\MeasureTraceTestsGarbageData.zip")]
        [InlineData(@"TestData\GarbageData.etl")]
        [InlineData(@"TestData\NothingHere.etl")]
        [InlineData(@"TestData\BOOT__GenericLightlyManaged.zip")]
        [InlineData(@"")]
        [InlineData(null)]
        public void BasicFileOpeningTest(string arg)
        {
            var pathToTest = string.Empty;
            Trace trace;
            try
            {
                pathToTest = Path.Combine(Environment.CurrentDirectory, arg);
                using (var tj = new TraceJob(pathToTest))
                {
                    trace = tj.Measure();
                    Assert.NotNull(trace);
                }
                Assert.True(!Directory.Exists(pathToTest.Remove(pathToTest.Length - 4)));
            }
            catch (Exception e)
            {
                if (pathToTest.Contains("GarbageData.zip")) Assert.IsType<InvalidDataException>(e);
                else if (pathToTest.Contains("GarbageData.etl")) Assert.IsType<ApplicationException>(e);
                else if (arg == null) Assert.IsType<ArgumentNullException>(e);
                else Assert.IsType<FileNotFoundException>(e);
                return;
            }
            Assert.NotNull(trace);
        }

        [Theory]
        [InlineData(@"TestData\BOOT-REFERENCE__NormalLightlyManaged.zip")]
        public void BasicCalipersRunningTest(string relativePath)
        {
            var expectedCompletionSeconds = 45;
            var expectedCompletionTime = DateTime.UtcNow.AddSeconds(expectedCompletionSeconds);
            var sourcePath = Path.Combine(Environment.CurrentDirectory, relativePath);
            var destPath = Path.Combine(Path.GetTempPath(), nameof(BasicCalipersRunningTest) + ".zip");
            File.Copy(sourcePath, destPath, true);
            using (var tj = new TraceJob(destPath))
            {
                tj.StageForProcessing();
                tj.RegisterProcessorByType<GroupPolicyActionProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                //tj.RegisterProcessorByType<DiskIoProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterProcessorByType<NetworkInterfaceProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterProcessorByType<ProcessLifetimeProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterProcessorByType<SystemSleepProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterProcessorByType<ServiceTransitionProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterProcessorByType<NetworkInterfaceProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterProcessorByType<DiskProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterCaliperByType<WinlogonSubscriber>();
                tj.RegisterCaliperByType<MeasureTrace.Calipers.TerminalSession>();
                //tj.RegisterCaliperByType<CpuSampled>();
                tj.RegisterCaliperByType<BootPhase>();
                var t = tj.Measure();
                Assert.NotNull(t);
                //Assert.True(expectedCompletionTime > DateTime.UtcNow);
                Assert.NotEmpty(t.GetMeasurements<IMeasurement>());
                Assert.NotEmpty(t.GetMeasurements<WinlogonSubscriberTask>());
                Assert.NotEmpty(t.GetMeasurements<GroupPolicyAction>());
                //Assert.NotEmpty(t.GetMeasurements<DiskIo>());
                //Assert.NotEmpty(t.GetMeasurements<MeasureTrace.TraceModel.CpuSampled>());
                Assert.NotEmpty(t.GetMeasurements<ProcessLifetime>());
                Assert.NotEmpty(t.GetMeasurements<MeasureTrace.TraceModel.BootPhase>());
                Assert.NotEmpty(t.GetMeasurements<ServiceTransition>());
                Assert.NotEmpty(t.GetMeasurements<MeasureTrace.TraceModel.TerminalSession>());
                Assert.NotEmpty(t.GetMeasurements<NetworkInterface>());
                Assert.NotEmpty(t.GetMeasurements<PhysicalDisk>());
                Assert.NotEmpty(t.GetMeasurements<LogicalDisk>());
                Assert.False(t.GetMeasurements<SystemSleep>().Any());
                Assert.True(string.Equals(t.PackageFileNameFull, destPath, StringComparison.OrdinalIgnoreCase));
                var packageRelName = Path.GetFileName(destPath);
                Assert.True(string.Equals(t.PackageFileNameFull, destPath, StringComparison.OrdinalIgnoreCase));
                Assert.True(string.Equals(Path.GetFileName(t.PackageFileNameFull), packageRelName, StringComparison.OrdinalIgnoreCase));

            }
        }

        [Theory]
        [InlineData(@"TestData\BOOT-REFERENCE__NormalLightlyManaged.zip")]
        public void CaliperOrderShouldNotMatter(string relativePath)
        {
            var sourcePath = Path.Combine(Environment.CurrentDirectory, relativePath);
            var destPath = Path.Combine(Path.GetTempPath(), nameof(CaliperOrderShouldNotMatter) + ".zip");
            File.Copy(sourcePath, destPath, true);
            int wlstCount;
            int tsCount;
            int bpCount;

            using (var tj = new TraceJob(destPath))
            {
                tj.StageForProcessing();
                tj.RegisterCaliperByType<WinlogonSubscriber>();
                tj.RegisterCaliperByType<MeasureTrace.Calipers.TerminalSession>();
                tj.RegisterCaliperByType<BootPhase>();
                var t = tj.Measure();
                wlstCount = t.GetMeasurements<MeasureTrace.TraceModel.WinlogonSubscriberTask>().Count();
                tsCount = t.GetMeasurements<MeasureTrace.TraceModel.TerminalSession>().Count();
                bpCount = t.GetMeasurements<MeasureTrace.TraceModel.BootPhase>().Count();
                Assert.True(bpCount == 5);
            }
            using (var tj = new TraceJob(destPath))
            {
                tj.StageForProcessing();
                tj.RegisterCaliperByType<BootPhase>();
                var t = tj.Measure();
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.TerminalSession>().Count() == tsCount);
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.BootPhase>().Count() == bpCount);
            }
            using (var tj = new TraceJob(destPath))
            {
                tj.StageForProcessing();
                tj.RegisterCaliperByType<BootPhase>();
                tj.RegisterCaliperByType<WinlogonSubscriber>();
                tj.RegisterCaliperByType<MeasureTrace.Calipers.TerminalSession>();
                var t = tj.Measure();
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.WinlogonSubscriberTask>().Count() == wlstCount);
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.TerminalSession>().Count() == tsCount);
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.BootPhase>().Count() == bpCount);
            }
        }
    }
}