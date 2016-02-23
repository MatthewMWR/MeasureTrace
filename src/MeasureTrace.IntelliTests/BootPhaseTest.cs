// <copyright file="BootPhaseTest.cs" company="@MatthewMwr">Copyright ©  2015</copyright>
using System;
using MeasureTrace.TraceModel;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MeasureTrace.TraceModel.IntelliTests
{
    /// <summary>This class contains parameterized unit tests for BootPhase</summary>
    [PexClass(typeof(BootPhase))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class BootPhaseTest
    {
    }
}
