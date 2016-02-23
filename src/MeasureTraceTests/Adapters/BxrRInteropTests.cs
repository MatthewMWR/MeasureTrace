using MeasureTrace.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasureTrace.TraceModel;
using Xunit;

namespace MeasureTrace.Adapters.Tests
{
    public class BxrRInteropTests
    {
        [Fact]
        public void PopulateCoreTraceAttributesFromPackageTest()
        {
            var testFileName = "BxrR__S-ome_Machine__BOOT__2015-08-21_11-50-37__Z2.zip";
            var trace = new Trace();
            BxrRInterop.PopulateCoreTraceAttributesFromPackage(trace);
            Assert.True(string.Equals(trace.ComputerName, "S-ome_Machine", StringComparison.OrdinalIgnoreCase));
            Assert.True( trace.TracePackageTime.Year == 2015);
            Assert.True(trace.TracePackageTime.Month == 08);
            Assert.True(trace.TracePackageTime.Minute == 50);
        }
    }
}