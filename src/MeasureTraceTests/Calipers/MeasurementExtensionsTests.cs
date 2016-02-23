// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using MeasureTrace.Adapters;
using MeasureTrace.Calipers;
using MeasureTrace.TraceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BootPhase = MeasureTrace.TraceModel.BootPhase;

namespace MeasureTraceTests.Calipers
{
    [TestClass]
    public class MeasurementExtensionsTests
    {
        [TestMethod]
        public void ResolveQualityTest()
        {
            try
            {
                MeasurementExtensions.ResolveQuality(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof (ArgumentNullException));
            }

            Assert.IsTrue(MeasurementExtensions.ResolveQuality(new BootPhase {DurationMSec = -1}) ==
                          MeasurementQuality.Unreliable);
            Assert.IsTrue(
                MeasurementExtensions.ResolveQuality(new BootPhase
                {
                    DurationMSec = WptInterop.WptMagicNumberNullPostBootDuration
                }) == MeasurementQuality.Unreliable);
            Assert.IsTrue(
                MeasurementExtensions.ResolveQuality(new BootPhase
                {
                    DurationMSec = WptInterop.WptMagicNumberUnboundDuration
                }) == MeasurementQuality.Unreliable);
            Assert.IsTrue(MeasurementExtensions.ResolveQuality(new BootPhase()) == MeasurementQuality.DefaultUsable);
        }
    }
}