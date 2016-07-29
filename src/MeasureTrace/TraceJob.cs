// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly int _deleteZipOutPathMaxTry = 3;
        private readonly string _callerSuppliedPath;
        private int _deleteZipOutPathCurrentTry = 1;
        private string _processingPath;
        private TracePackageType _tracePackageType;
        private DirectoryInfo _zipOutPath;
        public Action<IMeasurement> OnNewMeasurementAny;
        public IList<ProcessorBase> V1ProcessorsInternallyOwned = new List<ProcessorBase>();

        public TraceJob([NotNull] string etlPath)
        {
            if (string.IsNullOrWhiteSpace(etlPath)) throw new FileNotFoundException();
            Trace = new Trace();
            MeasurementsInProgress = new ConcurrentBag<IMeasurement>();
            UserData = new ConcurrentDictionary<object, object>();
            _callerSuppliedPath = etlPath;
        }

        [UsedImplicitly]
        public TraceJob(Trace sparseTrace)
        {
            if (sparseTrace == null) throw new ArgumentNullException(nameof(sparseTrace));
            Trace = sparseTrace;
            if (string.IsNullOrWhiteSpace(sparseTrace.PackageFileNameFull)) throw new FileNotFoundException();
            MeasurementsInProgress = new ConcurrentBag<IMeasurement>();
            UserData = new ConcurrentDictionary<object, object>();
            _callerSuppliedPath = sparseTrace.PackageFileNameFull;
        }

        public Trace Trace { get; }

        public ETWTraceEventSource EtwTraceEventSource { get; set; }

        public ConcurrentBag<IMeasurement> MeasurementsInProgress { get; private set; }
        public ConcurrentDictionary<object, object> UserData { get; private set; }

        public IList<Tuple<Type, ProcessorBase>> V1ProcessorList { get; } = new List<Tuple<Type, ProcessorBase>>();

        public void Dispose()
        {
            EtwTraceEventSource?.Dispose();
            if (_zipOutPath == null) return;
            while ( Directory.Exists(_zipOutPath.FullName) && _deleteZipOutPathCurrentTry <= _deleteZipOutPathMaxTry)
            {
                _deleteZipOutPathCurrentTry++;
                try
                {
                    _zipOutPath.Delete(true);
                }
                catch (Exception e)
                {
                    Logging.LogDebugMessage(e.Message);
                    Thread.Sleep(1000);
                }
            }
            if (Directory.Exists(_zipOutPath.FullName))
                Logging.LogDebugMessage($"Temp dir could not be deleted automatically {_zipOutPath.FullName}");
        }

        public Trace Measure()
        {
            if (EtwTraceEventSource == null) StageForProcessing();
            if (EtwTraceEventSource == null) throw new InvalidOperationException("Event source is null");
            OnNewMeasurementAny += m => Trace.AddMeasurement(m);
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
            return Trace;
        }

        public void RegisterCaliperByType<TCaliper>(TCaliper caliper) where TCaliper : ICaliper, new()
        {
            RegisterCaliperByType<TCaliper>();
        }

        public void RegisterCaliperByType<TCaliper>() where TCaliper : ICaliper, new()
        {
            var c = new TCaliper();
            _calipers.Add(c);
        }

        public void RegisterCaliper(ICaliper caliper)
        {
            _calipers.Add(caliper);
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

        public void StageForProcessing()
        {
            var etlPath = _callerSuppliedPath;
            //  If the path does not exist or is blank, etc. IO exceptions will be thrown
            if (string.IsNullOrWhiteSpace(etlPath)) throw new ArgumentNullException(nameof(etlPath));
            var fileInfo = new FileInfo(etlPath);
            if (fileInfo == null) throw new FileNotFoundException(etlPath);
            if (!fileInfo.Exists) throw new FileNotFoundException(etlPath);
            _tracePackageType = TraceJobExtension.ResolvePackageType(etlPath);
            var adapter = TraceJobExtension.GetPackageAdapter(_tracePackageType);
            adapter.PopulateTraceAttributesFromFileName(Trace, etlPath);
            if (_tracePackageType == TracePackageType.GenericEtl)
            {
                _processingPath = fileInfo.FullName;
            }
            if (_tracePackageType == TracePackageType.BxrRZip || _tracePackageType == TracePackageType.GenericZip ||
                _tracePackageType == TracePackageType.IcuZip)
            {
                var zipPath = fileInfo.FullName;
                _zipOutPath = Zip.UnzipPackage(zipPath);
                var tempEtlFileInfos = _zipOutPath.EnumerateFiles("*.etl");
                var firstEtl = tempEtlFileInfos.FirstOrDefault();
                if (firstEtl == null) throw new FileNotFoundException(_zipOutPath.FullName);
                _processingPath = firstEtl.FullName;
            }
            EtwTraceEventSource = new ETWTraceEventSource(_processingPath);
            if (_zipOutPath != null) adapter.PopulateTraceAttributesFromPackageContents(Trace, _zipOutPath.FullName);
            Trace.TraceSessionStart = EtwTraceEventSource.SessionStartTime;
        }

        public void OnNewMeasurementOfType<TMeasurement>(Action<TMeasurement> myDelegate)
            where TMeasurement : IMeasurement
        {
            OnNewMeasurementAny +=
                measurement1 => { if (measurement1 is TMeasurement) myDelegate.DynamicInvoke(measurement1); };
        }
    }
}