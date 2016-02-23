// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using MeasureTrace;
using MeasureTrace.Calipers;
using MeasureTrace.TraceModel;
using Xunit;

namespace MeasureTraceTests.Calipers
{
    public class CpuSampledCaliperTests
    {
        [Theory]
        [InlineData(@"TestData\BOOT__GenericLightlyManaged.zip")]
        public void CpuSampledCaliperTest(string relativePath)
        {
            var sourcePath = Path.Combine(Environment.CurrentDirectory, relativePath);
            var destPath = Path.Combine(Path.GetTempPath(), nameof(CpuSampledCaliperTest) + ".zip");
            File.Copy(sourcePath, destPath, true);
            using (var tj = new TraceJob(destPath))
            {
                tj.RegisterCaliperByType<MeasureTrace.Calipers.CpuSampled>(null);
                var t = tj.Measure();
                Assert.NotNull(t);
                Assert.True(t.GetMeasurements<MeasureTrace.TraceModel.CpuSampled>().OrderBy(m => m.Weight).Last().ProcessName == "mscorsvw");
                Assert.True(
                    t.GetMeasurements<MeasureTrace.TraceModel.CpuSampled>()
                        .Where(q => q.ProcessName == "MonitoringHost" && q.Weight > 0.03)
                        .Any());
            }
        }
    }
}