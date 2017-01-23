//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.IO;
using MeasureTrace;
using MeasureTrace.Adapters;
using MeasureTrace.Calipers;
using MeasureTrace.TraceModel;
using Xunit;

namespace MeasureTraceTests.Calipers
{
    public class GroupPolicyProcessorTest
    {
        [Theory]
        [InlineData(@"TestData\BOOT-REFERENCE__NormalLightlyManaged.zip")]
        public void GroupPolicyTest(string relativePath)
        {
            var sourcePath = Path.Combine(Environment.CurrentDirectory, relativePath);
            var destPath = Path.Combine(Path.GetTempPath(), nameof(GroupPolicyTest) + ".zip");
            File.Copy(sourcePath, destPath, true);
            using (var tj = new TraceJob(destPath))
            {
                tj.StageForProcessing();
                tj.RegisterProcessorByType<GroupPolicyActionProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
                var t = tj.Measure();
                Assert.NotNull(t);
                Assert.NotEmpty(t.GetMeasurements<GroupPolicyAction>());
            }
        }
    }
}