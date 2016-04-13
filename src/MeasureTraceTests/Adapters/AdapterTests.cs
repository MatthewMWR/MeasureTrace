using System;
using System.IO;
using System.Linq;
using MeasureTrace;
using MeasureTrace.Adapters;
using Xunit;

namespace MeasureTraceTests.Adapters
{
    public class AdapterTests
    {
        [Theory]
        [InlineData(@"20160107-103455_CLINTH-SP3_UserInitiated.zip")]
        [InlineData(@"20160212-102918_WIN7-32_Clue_ProcessorTimeGt90.zip")]
        [InlineData(@"BxrR__BX-WIN81__BOOT__2016-02-28_17-47-29__Z2.zip")]
        public void TestIcuPackageMetadata(string packageFileName)
        {
            var packagePath = Path.Combine(Environment.CurrentDirectory, "TestData", packageFileName);
            using (var traceJob = new TraceJob(packagePath))
            {
                traceJob.StageForProcessing();
                var trace = traceJob.Trace;
                Assert.True(
                    new[] {"CLINTH-SP3", "WIN7-32", "BX-WIN81"}.Contains(trace.ComputerName,
                        StringComparer.OrdinalIgnoreCase)
                    );
                Assert.True(trace.TracePackageTime.Year == 2016);
                if (packageFileName.Contains("UserInitiated"))
                {
                    Assert.True(trace.GetTraceAttributes().Any(ta => ta.Name == CluePackageAdapter.NameOfIcuUserNoteAttribute));
                }
                if (!packageFileName.StartsWith("BxrR", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.True(
                        trace.GetTraceAttributes().Any(
                            ta => ta.Name == CluePackageAdapter.NameOfIcuMetaEmailReportToAttribute));
                }
                if (packageFileName.StartsWith("BxrR", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.True(
                        trace.GetTraceAttributes().Any(
                            ta => ta.Name == "OSInstallDateWMI" && ta.StringValue != null && ta.DateTimeValue != null));
                }
                Assert.True(
                    trace.GetTraceAttributes().Any(
                        ta => string.Equals(ta.Name, "Trigger", StringComparison.OrdinalIgnoreCase)));
            }
        }
    }
}