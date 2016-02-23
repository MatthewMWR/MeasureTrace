using System;
using MeasureTrace.TraceModel;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MeasureTrace.TraceModel.Tests
{
    /// <summary>This class contains parameterized unit tests for BootPhase</summary>
    [TestClass]
    [PexClass(typeof(BootPhase))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class BootPhaseTest
    {
    }
}
