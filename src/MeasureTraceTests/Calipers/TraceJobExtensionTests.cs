//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.IO;
using System.Linq;
using MeasureTrace;
using MeasureTrace.Adapters;
using Xunit;

namespace MeasureTraceTests.Calipers
{
    public class TraceJobExtensionTests
    {
        [Theory]
        [InlineData(@"TestData\BOOT-REFERENCE__NormalLightlyManaged.zip")]
        public void RegisterCalipersAllKnownTest(string relativePath)
        {
            var expectedGapInMeasurementTypes = 1;
            if (!WptInterop.IsXperfInstalled()) expectedGapInMeasurementTypes++;
            var sourcePath = Path.Combine(Environment.CurrentDirectory, relativePath);
            var destPath = Path.Combine(Path.GetTempPath(), nameof(RegisterCalipersAllKnownTest) + ".zip");
            File.Copy(sourcePath, destPath, true);
            using (var tj = new TraceJob(destPath))
            {
                //tj.RegisterProcessorByType<BootPhaseProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                tj.StageForProcessing();
                tj.RegisterCalipersAllKnown();
                var t = tj.Measure();
                Assert.NotNull(t);
                var knownMeasurementTypes = t.GetKnownMeasurementTypes().ToList();
                var presentMeasurementTypes = t.GetPresentMeasurementTypes().ToList();
                Assert.True(knownMeasurementTypes.Count == presentMeasurementTypes.Count + expectedGapInMeasurementTypes);
            }
        }
    }
}