// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using MeasureTrace.Adapters;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;

namespace MeasureTrace
{
    public sealed class TraceJob : IDisposable
    {
        private readonly IList<ICaliper> _calipers = new List<ICaliper>();
        private readonly Trace _trace = new Trace();
        private string _processingPath;
        private string _stablePath;
        private TracePackageType _tracePackageType;
        private DirectoryInfo _zipOutPath;
        public Action<IMeasurement> OnNewMeasurementAny;
        public IList<ProcessorBase> V1ProcessorsInternallyOwned = new List<ProcessorBase>();

        public TraceJob([NotNull] string etlPath)
        {
            if (string.IsNullOrWhiteSpace(etlPath)) throw new FileNotFoundException();
            _trace = new Trace();
            MeasurementsInProgress = new ConcurrentBag<IMeasurement>();
            UserData = new ConcurrentDictionary<object, object>();
            ResolveDataPaths(etlPath);
            EtwTraceEventSource = new ETWTraceEventSource(_processingPath);
            PopulateTraceCoreAttributes();
        }

        private void PopulateTraceCoreAttributes()
        {
            _trace.TraceSessionStart = EtwTraceEventSource.SessionStartTime;
            if(_tracePackageType == TracePackageType.IcuZip) IcuInterop.PopulateCoreTraceAttributesFromPackage(_trace);
            else if (_tracePackageType == TracePackageType.BxrRZip) BxrRInterop.PopulateCoreTraceAttributesFromPackage(_trace);
        }

        //public TraceJob(Trace sparseTrace)
        //{
        //    if (sparseTrace== null) throw new ArgumentNullException(nameof(sparseTrace));
        //    _trace = sparseTrace;
        //    if (string.IsNullOrWhiteSpace(sparseTrace.DataPathStable)) throw new FileNotFoundException();
        //    MeasurementsInProgress = new ConcurrentBag<IMeasurement>();
        //    UserData = new ConcurrentDictionary<object, object>();
        //    ResolveDataPaths(sparseTrace.DataPathStable);
        //    EtwTraceEventSource = new ETWTraceEventSource(_processingPath);
        //}

        public ETWTraceEventSource EtwTraceEventSource { get; set; }

        public ConcurrentBag<IMeasurement> MeasurementsInProgress { get; private set; }
        public ConcurrentDictionary<object, object> UserData { get; private set; }

        public IList<Tuple<Type, ProcessorBase>> V1ProcessorList { get; } = new List<Tuple<Type, ProcessorBase>>();

        public void Dispose()
        {
            EtwTraceEventSource?.Dispose();
            if (_zipOutPath != null) Directory.Delete(_zipOutPath.FullName, true);
        }

        public Trace Measure()
        {
            PopulateTraceCoreAttributes();

            OnNewMeasurementAny += m => _trace.AddMeasurement(m);
            foreach (var c in _calipers)
            {
                c.RegisterFirstPass(this);
            }
            foreach (var pt in V1ProcessorList)
            {
                pt.Item2.PreTraceEventProcessing?.Invoke();
            }
            EtwTraceEventSource.Process();
            foreach (var pt in V1ProcessorList)
            {
                pt.Item2.PostTraceEventProcessing?.Invoke();
            }
            EtwTraceEventSource.Dispose();
            EtwTraceEventSource = new ETWTraceEventSource(_processingPath);
            foreach (var c in _calipers)
            {
                c.RegisterSecondPass(this);
            }
            EtwTraceEventSource.Process();
            return _trace;
        }

        public void RegisterCaliperByType<TCaliper>(TCaliper caliper) where TCaliper : ICaliper, new()
        {
            var c = new TCaliper();
            _calipers.Add(c);
        }

        public void RegisterCaliperByType<TCaliper>() where TCaliper : ICaliper, new()
        {
            var c = new TCaliper();
            _calipers.Add(c);
        }

        public void OnNewMeasurementOfType<TMeasurement>(Delegate myDelegate)
            where TMeasurement : IMeasurement
        {
            OnNewMeasurementAny +=
                measurement1 => { if (measurement1 is TMeasurement) myDelegate.DynamicInvoke(measurement1); };
        }

        public void PublishMeasurement<TMeasurement>(TMeasurement measurement) where TMeasurement : IMeasurement
        {
            OnNewMeasurementAny(measurement);
        }

        public void PublishMeasurement<TMeasurement>(IEnumerable<TMeasurement> measurements)
            where TMeasurement : IMeasurement
        {
            foreach (var m in measurements)
            {
                PublishMeasurement(m);
            }
        }


        private void ResolveDataPaths([NotNull] string etlPath)
        {
            //  If the path does not exist or is blank, etc. IO exceptions will be thrown
            if (string.IsNullOrWhiteSpace(etlPath)) throw new ArgumentNullException(nameof(etlPath));
            var fileInfo = new FileInfo(etlPath);
            if (fileInfo == null) throw new FileNotFoundException(etlPath);
            if (!fileInfo.Exists) throw new FileNotFoundException(etlPath);
            if (string.Equals(fileInfo.Extension, ".etl", StringComparison.OrdinalIgnoreCase))
            {
                _stablePath = fileInfo.FullName;
                _processingPath = fileInfo.FullName;
                _tracePackageType = TracePackageType.GenericEtl;
            }
            else if (string.Equals(fileInfo.Extension, ".zip", StringComparison.OrdinalIgnoreCase))
            {
                var zipPath = fileInfo.FullName;
                _zipOutPath = Zip.UnzipPackage(zipPath);
                var tempEtlFileInfos = _zipOutPath.EnumerateFiles("*.etl");

                var firstEtl = tempEtlFileInfos.FirstOrDefault();
                if (firstEtl == null) throw new FileNotFoundException(_zipOutPath.FullName);
                _stablePath = zipPath;
                _processingPath = firstEtl.FullName;
                if( Regex.IsMatch(fileInfo.Name, BxrRInterop.BxrRFileNamePattern, RegexOptions.IgnoreCase)) _tracePackageType = TracePackageType.BxrRZip;
                else if (Regex.IsMatch(fileInfo.Name, IcuInterop.IcuFileNamePattern, RegexOptions.IgnoreCase)) _tracePackageType = TracePackageType.IcuZip;
                else _tracePackageType = TracePackageType.GenericZip;
            }
            _trace.DataPathDuringProcessing = _processingPath;
            _trace.DataPathStable = _stablePath;
        }

        public void OnNewMeasurementOfType<TMeasurement>(Action<TMeasurement> myDelegate)
            where TMeasurement : IMeasurement
        {
            OnNewMeasurementAny +=
                measurement1 => { if (measurement1 is TMeasurement) myDelegate.DynamicInvoke(measurement1); };
        }
    }
}