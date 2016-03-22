﻿// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private int _deleteZipOutPathCurrentTry = 1;
        private readonly int _deleteZipOutPathMaxTry = 3;
        private int _populateTraceCoreAttributesCallCount;
        private string _processingPath;
        private string _stablePath;
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
            ResolveDataPaths(etlPath);
            EtwTraceEventSource = new ETWTraceEventSource(_processingPath);
        }

        [UsedImplicitly]
        public TraceJob(Trace sparseTrace)
        {
            if (sparseTrace == null) throw new ArgumentNullException(nameof(sparseTrace));
            Trace = sparseTrace;
            if (string.IsNullOrWhiteSpace(sparseTrace.DataPathStable)) throw new FileNotFoundException();
            MeasurementsInProgress = new ConcurrentBag<IMeasurement>();
            UserData = new ConcurrentDictionary<object, object>();
            ResolveDataPaths(sparseTrace.DataPathStable);
            EtwTraceEventSource = new ETWTraceEventSource(_processingPath);
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
            while (_zipOutPath.Exists && _deleteZipOutPathCurrentTry <= _deleteZipOutPathMaxTry)
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
            if (_zipOutPath.Exists)
                Logging.LogDebugMessage($"Temp dir could not be deleted automatically {_zipOutPath.FullName}");
        }

        public void PopulateTraceCoreAttributes()
        {
            _populateTraceCoreAttributesCallCount++;
            Trace.TraceSessionStart = EtwTraceEventSource.SessionStartTime;
            IPackageAdapter adapter = null;
            if (_tracePackageType == TracePackageType.IcuZip) adapter = new CluePackageAdapter();
            else if (_tracePackageType == TracePackageType.BxrRZip) adapter = new BxrRPackageAdapter();
            if (adapter != null) adapter.PopulateTraceAttributesFromFileName(Trace, Trace.DataFileNameRelative);
            if (adapter != null) adapter.PopulateTraceAttributesFromPackageContents(Trace, _zipOutPath.FullName);
        }

        public Trace Measure()
        {
            if (_populateTraceCoreAttributesCallCount == 0) PopulateTraceCoreAttributes();

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
                if (Regex.IsMatch(fileInfo.Name, BxrRPackageAdapter.BxrRFileNamePattern, RegexOptions.IgnoreCase))
                    _tracePackageType = TracePackageType.BxrRZip;
                else if (Regex.IsMatch(fileInfo.Name, CluePackageAdapter.IcuFileNamePattern, RegexOptions.IgnoreCase))
                    _tracePackageType = TracePackageType.IcuZip;
                else _tracePackageType = TracePackageType.GenericZip;
            }
            Trace.DataPathDuringProcessing = _processingPath;
            Trace.DataPathStable = _stablePath;
            Trace.DataFileNameRelative = Path.GetFileName(_stablePath);
        }

        public void OnNewMeasurementOfType<TMeasurement>(Action<TMeasurement> myDelegate)
            where TMeasurement : IMeasurement
        {
            OnNewMeasurementAny +=
                measurement1 => { if (measurement1 is TMeasurement) myDelegate.DynamicInvoke(measurement1); };
        }
    }
}