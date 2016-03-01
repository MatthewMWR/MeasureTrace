// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using MeasureTrace;
using MeasureTrace.TraceModel;
using Xunit;
using BootPhase = MeasureTrace.Calipers.BootPhase;

namespace MeasureTraceTests.Calipers
{
    public class BootPhaseCaliperTests
    {
        [Theory]
        [InlineData(@"TestData\BOOT-REFERENCE__NormalLightlyManaged.zip")]
        public void BootPhaseCaliperTest(string relativePath)
        {
            var sourcePath = Path.Combine(Environment.CurrentDirectory, relativePath);
            var destPath = Path.Combine(Path.GetTempPath(), nameof(BootPhaseCaliperTest) + ".zip");
            File.Copy(sourcePath, destPath, true);
            using (var tj = new TraceJob(destPath))
            {
                //tj.RegisterProcessorByType<BootPhaseProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.RegisterCaliperByType<BootPhase>();
                var t = tj.Measure();
                Assert.NotNull(t);
                Assert.True(
                    t.GetMeasurements<MeasureTrace.TraceModel.BootPhase>().Count(p => p.BootPhaseObserver == BootPhaseObserver.MeasureTrace) ==
                    5);
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.BootPhase>().First(bp => bp.BootPhaseType == BootPhaseType.FromPowerOnUntilDesktopResponsive).DurationMSec > 38000);
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.BootPhase>().First(bp => bp.BootPhaseType == BootPhaseType.FromPowerOnUntilDesktopResponsive).DurationMSec < 41000);
                //Assert.True(
                //    t.GetMeasurements<BootPhase>()
                //        .Count(p => p.BootPhaseObserver == BootPhaseObserver.WpaExporterBootPhases) == 6);
                Assert.True(t.GetMeasurements<TerminalSession>().Count() == 3);
            }
        }
    }
}