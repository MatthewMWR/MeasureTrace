// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using MeasureTrace.Adapters;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;

#pragma warning disable 618

namespace MeasureTrace.Calipers
{
    public class BootPhase : ICaliper
    {
        //private TraceJob _traceJob;
        private const string PerfTrackProviderName = "Microsoft-Windows-Diagnostics-PerfTrack";
        //private const string PerfTrackIdleDetectionStopEventName = "IdleDetection_Stop";
        //private const int PerfTrackIdleDetectionStopEventId = 1103;
        private const int PerfTrackIdleDetectionInfoEventId = 1110;
        private const string WinlogonProviderName = "Microsoft-Windows-Winlogon";
        private const int WinlogonSystemBootEventId = 5007;
        private const int WinlogonWelcomeScreenStartId = 201;
        private const int IdleAccumulationCutoffMs = 3000;

        private readonly ICollection<TraceModel.BootPhase> _alreadyRegisteredBootPhases =
            new List<TraceModel.BootPhase>();

        private int _countOfPowerOnToReadyForLogon;

        public void RegisterFirstPass(TraceJob traceJob)
        {
            //_traceJob = traceJob;
        }

        public void RegisterSecondPass(TraceJob traceJob)
        {
            traceJob.RegisterProcessorByType<TerminalSessionProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);

            traceJob.OnNewMeasurementOfType<TraceModel.BootPhase>(bp => _alreadyRegisteredBootPhases.Add(bp));

            traceJob.OnNewMeasurementOfType<TerminalSession>(ts =>
            {
                if (
                    _alreadyRegisteredBootPhases.Any(
                        em => em.BootPhaseType == BootPhaseType.FromLogonUntilDesktopAppears)) return;
                if (ts.ExplorerProcessId == 0) return;
                traceJob.PublishMeasurement(
                    new TraceModel.BootPhase
                    {
                        BootPhaseObserver = BootPhaseObserver.MeasureTrace,
                        BootPhaseType = BootPhaseType.FromLogonUntilDesktopAppears,
                        DurationMSec = ts.LogonCredentialEntryToShellReady
                    }
                    );
                traceJob.PublishMeasurement(
                    new TraceModel.BootPhase
                    {
                        BootPhaseObserver = BootPhaseObserver.MeasureTrace,
                        BootPhaseType = BootPhaseType.FromPowerOnUntilDesktopAppears,
                        DurationMSec = ts.ShellReadyOffsetMSec
                    }
                    );
            });

            traceJob.EtwTraceEventSource.Registered.AddCallbackForProviderEvents((pn, en) =>
            {
                if (!string.Equals(pn, WinlogonProviderName, StringComparison.OrdinalIgnoreCase))
                    return EventFilterResponse.RejectProvider;
                return EventFilterResponse.AcceptEvent;
            }, e =>
            {
                if ((WinlogonSystemBootEventId == (int) e.ID || WinlogonWelcomeScreenStartId == (int) e.ID) &&
                    _countOfPowerOnToReadyForLogon < 1)
                {
                    _countOfPowerOnToReadyForLogon++;
                    traceJob.PublishMeasurement(new TraceModel.BootPhase
                    {
                        BootPhaseObserver = BootPhaseObserver.MeasureTrace,
                        BootPhaseType = BootPhaseType.FromPowerOnUntilReadyForLogon,
                        DurationMSec = e.TimeStampRelativeMSec
                    });
                }
            });

            traceJob.EtwTraceEventSource.Registered.AddCallbackForProviderEvents((pn, en) =>
            {
                if (!string.Equals(pn, PerfTrackProviderName, StringComparison.OrdinalIgnoreCase))
                    return EventFilterResponse.RejectProvider;
                return EventFilterResponse.AcceptEvent;
            }, e =>
            {
                if (PerfTrackIdleDetectionInfoEventId == (int) e.ID &&
                    _alreadyRegisteredBootPhases.All(
                        bp => bp.BootPhaseType != BootPhaseType.FromDesktopAppearsUntilDesktopResponsive))
                {
                    var accumulatedIdleMs = Convert.ToInt32(e.PayloadValue(0));
                    if (IdleAccumulationCutoffMs > accumulatedIdleMs) return;
                    var postBootEndOffsetMinusAccumulatedIdle = e.TimeStampRelativeMSec - accumulatedIdleMs;
                    var bootToDesktop =
                        _alreadyRegisteredBootPhases.First(
                            bp => bp.BootPhaseType == BootPhaseType.FromPowerOnUntilDesktopAppears);
                    var postBootOnlyRawDurationMs = postBootEndOffsetMinusAccumulatedIdle - bootToDesktop.DurationMSec;
                    var postBootDurationCleaned = CalculateRollOffPostBootValue(postBootOnlyRawDurationMs.Value);
                    var desktopAppearsToDestkopResponsive = new TraceModel.BootPhase
                    {
                        BootPhaseObserver = BootPhaseObserver.MeasureTrace,
                        BootPhaseType = BootPhaseType.FromDesktopAppearsUntilDesktopResponsive,
                        DurationMSec = postBootDurationCleaned
                    };
                    traceJob.PublishMeasurement(desktopAppearsToDestkopResponsive);
                    traceJob.PublishMeasurement(new TraceModel.BootPhase
                    {
                        BootPhaseObserver = BootPhaseObserver.MeasureTrace,
                        BootPhaseType = BootPhaseType.FromPowerOnUntilDesktopResponsive,
                        DurationMSec = e.TimeStampRelativeMSec - (postBootOnlyRawDurationMs - postBootDurationCleaned)
                    });
                }
            });
        }

        /// <summary>
        ///     Provides a nice smooth rolloff of inflated PostBoot values for typical range
        /// </summary>
        /// <param name="durationMSec"></param>
        /// <returns></returns>
        private static double CalculateRollOffPostBootValue(double durationMSec)
        {
            var multiplier = 1 - Math.Sqrt(durationMSec/7000000);
            if (multiplier < 0.3) multiplier = 0.3;
            return durationMSec*multiplier;
        }
    }
}