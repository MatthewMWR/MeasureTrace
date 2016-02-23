using System;
using System.Linq;
using Xunit;

namespace MeasureTraceTests.Adapters
{
    public class IcuInteropTests
    {
        //public const string PathToTestIcuMetaData = @"C:\Users\mreyn\Downloads\20160107-103455_CLINTH-SP3_UserInitiated\20160107-103455_CLINTH-SP3_UserInitiated"
        [Fact]
        public void TestIcuEmailReportTo()
        {
            var pathToTestIcuMetaData =
                @"C:\Users\mreyn\Downloads\20160107-103455_CLINTH-SP3_UserInitiated\20160107-103455_CLINTH-SP3_UserInitiated";
            var result = MeasureTrace.Adapters.IcuInterop.GetIcuEmailReportToAddresses(pathToTestIcuMetaData);
            Assert.NotEmpty(result);
            Assert.True(string.Equals(result.First(), "clinth@microsoft.com", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void TestIcuUserNote()
        {
            var pathToTestIcuMetaData =
                @"C:\Users\mreyn\Downloads\20160107-103455_CLINTH-SP3_UserInitiated\20160107-103455_CLINTH-SP3_UserInitiated";
            var result = MeasureTrace.Adapters.IcuInterop.GetIseUserInitiatedNote(pathToTestIcuMetaData);
            Assert.NotEmpty(result);
            Assert.True(string.Equals(result.First(), "Test", StringComparison.OrdinalIgnoreCase));
        }
    }
}
