//// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using MeasureTrace.Adapters;
//using MeasureTrace.CalipersModel;
//using MeasureTrace.TraceModel;
//using Microsoft.Diagnostics.Tracing;

//namespace MeasureTrace.Calipers
//{
//    public class BootPhaseProcessor : ProcessorBase, IObserver<TraceEvent>, IObserver<IMeasurement>
//    {
//        private readonly List<TraceModel.BootPhase> _measurements = new List<TraceModel.BootPhase>();
//        //private bool _abort;
//        private int _winlogonSystemBootCount;

//        public void OnNext(IMeasurement measurement)
//        {
//            //if (_abort) return;
//            //var sleep = measurement as SystemSleep;
//            //if (sleep != null)
//            //{
//            //    _abort = true;
//            //}
//            var session = measurement as TerminalSession;
//            if (session != null)
//            {
//                MakeSessionDependentBootPhase(session, _measurements);

//                MakeConsolidatedBootPhasesAsNeeded(_measurements);
//            }
//        }

//        public void OnNext(TraceEvent value)
//        {
//            //if (_abort) return;
//            if ((int) value.ID == WinlogonDomainKnowledge.WinlogonSystemBootEventId && (int) value.PayloadValue(0) == 1 &&
//                _winlogonSystemBootCount < 1)
//            {
//                _winlogonSystemBootCount++;
//                RegisterSimpleBootPhase(value.TimeStampRelativeMSec, BootPhaseType.FromPowerOnUntilReadyForLogon,
//                    BootPhaseObserver.MeasureTrace);
//            }
//            else if ((int) value.ID == WinlogonDomainKnowledge.WinlogonWelcomeScreenStartId &&
//                     _winlogonSystemBootCount < 1)
//            {
//                //  This is a fallback trigger for traces which might not have the prior event
//                _winlogonSystemBootCount++;
//                RegisterSimpleBootPhase(value.TimeStampRelativeMSec, BootPhaseType.FromPowerOnUntilReadyForLogon,
//                    BootPhaseObserver.MeasureTrace);
//            }
//        }

//        public void OnCompleted()
//        {
//        }

//        private void MakeConsolidatedBootPhasesAsNeeded(List<TraceModel.BootPhase> measurements)
//        {
//            var fromBootToWelcome =
//                measurements.FirstOrDefault(m2 => m2.BootPhaseType == BootPhaseType.FromPowerOnUntilReadyForLogon);
//            var fromCredsEnteredToDesktopAppears =
//                measurements.FirstOrDefault(m => m.BootPhaseType == BootPhaseType.FromLogonUntilDesktopAppears);
//            if (fromBootToWelcome != null && fromCredsEnteredToDesktopAppears != null &&
//                _measurements.All(em => em.BootPhaseType != BootPhaseType.FromPowerOnUntilDesktopAppears))
//            {
//                var fromBootToDesktopAppears = new TraceModel.BootPhase
//                {
//                    BootPhaseObserver = BootPhaseObserver.MeasureTrace,
//                    BootPhaseType = BootPhaseType.FromPowerOnUntilDesktopAppears,
//                    DurationMSec = fromCredsEnteredToDesktopAppears.DurationMSec + fromBootToWelcome.DurationMSec
//                };
//                RegisterMeasurement(fromBootToDesktopAppears);
//            }
//            var fromDesktopAppearsToDesktopResponsive =
//                measurements.FirstOrDefault(
//                    m => m.BootPhaseType == BootPhaseType.FromDesktopAppearsUntilDesktopResponsive);
//            if (fromDesktopAppearsToDesktopResponsive != null && fromBootToWelcome != null &&
//                fromCredsEnteredToDesktopAppears != null &&
//                measurements.All(m => m.BootPhaseType != BootPhaseType.FromDesktopAppearsUntilDesktopResponsive))
//            {
//                var fromBootToDesktopResponsive = new TraceModel.BootPhase
//                {
//                    BootPhaseObserver = BootPhaseObserver.MeasureTrace,
//                    BootPhaseType = BootPhaseType.FromPowerOnUntilDesktopResponsive,
//                    DurationMSec =
//                        fromBootToWelcome.DurationMSec + fromCredsEnteredToDesktopAppears.DurationMSec +
//                        fromDesktopAppearsToDesktopResponsive.DurationMSec
//                };
//                RegisterMeasurement(fromBootToDesktopResponsive);
//            }
//        }

//        private void MakeSessionDependentBootPhase(TerminalSession session, List<TraceModel.BootPhase> measurements)
//        {
//            if (measurements.Any(em => em.BootPhaseType == BootPhaseType.FromLogonUntilDesktopAppears))
//                return;
//            if (session.ExplorerProcessId == 0) return;
//            var m = new TraceModel.BootPhase
//            {
//                BootPhaseObserver = BootPhaseObserver.MeasureTrace,
//                BootPhaseType = BootPhaseType.FromLogonUntilDesktopAppears,
//                DurationMSec = session.LogonCredentialEntryToShellReady
//            };
//            RegisterMeasurement(m);
//        }

//        public override void Initialize(TraceJob traceJob)
//        {
//            Subscriptions.Add(
//#pragma warning disable 618
//                traceJob.EtwTraceEventSource.Registered.Observe((providerName, eventName) =>
//#pragma warning restore 618
//                    providerName.Equals(WinlogonDomainKnowledge.WinlogonProviderName, StringComparison.OrdinalIgnoreCase)
//                        ? EventFilterResponse.AcceptEvent
//                        : EventFilterResponse.RejectProvider
//                    ).Subscribe(this)
//                );
//            traceJob.RegisterProcessorByType<TerminalSessionProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
//            traceJob.RegisterProcessorByType<SystemSleepProcessor>(ProcessorTypeCollisionOption.UseExistingIfFound);
//            //traceJob.RegisterProcessorByType<BootPhaseProcessorWpa>(ProcessorTypeCollisionOption.UseExistingIfFound);

//            Subscriptions.Add(
//                traceJob.GetRegisteredProcessors().First(p => p is TerminalSessionProcessor).Subscribe(this));
//            Subscriptions.Add(
//                traceJob.GetRegisteredProcessors().First(p => p is SystemSleepProcessor).Subscribe(this));
//            //Subscriptions.Add(traceJob.GetRegisteredProcessors().OfType<BootPhaseProcessorWpa>().First().Subscribe(this));
//            PreTraceEventProcessing += () =>
//            {
//                foreach (var m in WptInterop.GetWpaExporterBootPhases(traceJob.EtwTraceEventSource.LogFileName))
//                {
//                    RegisterMeasurement(m);
//                }
//                var postBoot = _measurements.FirstOrDefault(m => m.BootPhaseType == BootPhaseType.PostBoot);
//                if (postBoot?.DurationMSec == null) return;
//                if (postBoot.MeasurementQuality == MeasurementQuality.Unreliable) return;
//                var cleanedDuration = CalculateRollOffPostBootValue(postBoot.DurationMSec.Value);
//                var fromDesktopAppearsToDesktopResponsive = new TraceModel.BootPhase
//                {
//                    BootPhaseType = BootPhaseType.FromDesktopAppearsUntilDesktopResponsive,
//                    BootPhaseObserver = BootPhaseObserver.MeasureTrace,
//                    DurationMSec = cleanedDuration
//                };
//                RegisterMeasurement(fromDesktopAppearsToDesktopResponsive);
//            };
//        }

//        /// <summary>
//        ///     Provides a nice smooth rolloff of inflated PostBoot values for typical range
//        /// </summary>
//        /// <param name="durationMSec"></param>
//        /// <returns></returns>
//        private static double CalculateRollOffPostBootValue(double durationMSec)
//        {
//            var multiplier = 1 - Math.Sqrt(durationMSec/7000000);
//            if (multiplier < 0.3) multiplier = 0.3;
//            return durationMSec*multiplier;
//        }

//        private void RegisterSimpleBootPhase(double duration, BootPhaseType type, BootPhaseObserver context)
//        {
//            var m = new TraceModel.BootPhase
//            {
//                DurationMSec = duration,
//                BootPhaseType = type,
//                BootPhaseObserver = context
//            };
//            RegisterMeasurement(m);
//        }

//        protected override void RegisterMeasurement(IMeasurement measurement)
//        {
//            _measurements.Add((TraceModel.BootPhase) measurement);
//            base.RegisterMeasurement(measurement);
//        }
//    }
//}

