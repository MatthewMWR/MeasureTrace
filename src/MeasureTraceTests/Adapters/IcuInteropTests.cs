using MeasureTrace.Adapters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeasureTrace.TraceModel;
using Xunit;

namespace MeasureTrace.Adapters.Tests
{
    public class IcuInteropTests
    {
        [Theory]
        [InlineData("20160107-103455_CLINTH-SP3_UserInitiated")]
        [InlineData("20160212-102918_WIN7-32_Clue_ProcessorTimeGt90.zip")]
        [InlineData("20160211-133348_ROGERSO_W540_ICU_ProcessorTimeGt90.zip")]
        public void PopulateInitialTraceValuesIcuTest(string arg)
        {
            var computernames = new[] {@"CLINTH-SP3",@"WIN7-32",@"ROGERSO_W540"};
            var triggerNames = new[] { @"UserInitiated", @"ProcessorTimeGt90" };
            var trace = new Trace();
            trace.DataFileNameRelative = arg;
            IcuInterop.PopulateCoreTraceAttributesFromPackage(trace);
            Assert.True(computernames.Contains(trace.ComputerName));
            Assert.True(triggerNames.Contains(trace.TraceAttributes.FirstOrDefault(a => string.Equals(a.Name, IcuInterop.IcuNameOfTriggerAttribute, StringComparison.OrdinalIgnoreCase))?.StringValue));
            Assert.True(trace.TracePackageTime.Year == 2016);
        }
    }
}