//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Generic;
using System.Linq;
using MeasureTrace.CalipersModel;
using MeasureTrace.TraceModel;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

namespace MeasureTrace.Calipers
{
    /// <summary>
    ///     Measures logon sessions (including special sessions like session 0
    ///     Requires a trace to have Winlogon events and kernel process events
    ///     LocalSessionManager events add detail where available
    /// </summary>
    public class TerminalSession : ICaliper
    {
        private const string WinlogonProviderName = "Microsoft-Windows-Winlogon";
        private const string LsmProviderName = "Microsoft-Windows-TerminalServices-LocalSessionManager";
        private const string ShellCoreProviderName = "Microsoft-Windows-Shell-Core";
        private const string WinlogonProcessNameNoExtension = "winlogon";
        private const string ExplorerProcessNameNoExtension = "explorer";
        private readonly List<int> _sessionsAlreadyRegistered = new List<int>();
        private readonly ICollection<TraceModel.TerminalSession> _sessionsPartial = new List<TraceModel.TerminalSession>();
        public IEnumerable<Type> DependsOnCalipers => new List<Type> { };

        public void RegisterFirstPass(TraceJob traceJob)
        {
            
        }

        public void RegisterSecondPass(TraceJob traceJob)
        {
            traceJob.EtwTraceEventSource.Kernel.ProcessStartGroup += processStartValue =>
            {
                TriageProcessStartEvent(processStartValue);
                RegisterFinishedSessions(false, traceJob);
            };
            traceJob.EtwTraceEventSource.Registered.AddCallbackForProviderEvents( 
                (pn,en) => {
                    if (string.Equals(pn, WinlogonProviderName, StringComparison.OrdinalIgnoreCase))
                    {
                        return EventFilterResponse.AcceptEvent;
                    }
                    else
                    {
                        return EventFilterResponse.RejectProvider;
                    }
                }, wlEvent => TriageWinlogonEvent(wlEvent)
            );
            traceJob.EtwTraceEventSource.Registered.AddCallbackForProviderEvents(
                (pn, en) => {
                    if (string.Equals(pn, ShellCoreProviderName, StringComparison.OrdinalIgnoreCase))
                    {
                        return EventFilterResponse.AcceptEvent;
                    }
                    else
                    {
                        return EventFilterResponse.RejectProvider;
                    }
                }, shellEvent => TriageShellCoreEvent(shellEvent)
            );
            traceJob.EtwTraceEventSource.Completed += () => RegisterFinishedSessions(true, traceJob);
        }

        private void TriageWinlogonEvent(TraceEvent value)
        {
            if ((int) value.ID == 1)
            {
                foreach (var session in _sessionsPartial.Where(s => s.WinlogonPid == value.ProcessID))
                {
                    session.LastAuthenticateUserStartOffsetMSec = value.TimeStampRelativeMSec;
                }
            }
            if ((int) value.ID == 5007 || (int) value.ID == 202)
            {
                foreach (
                    var session in
                        _sessionsPartial.Where(
                            s => s.WinlogonPid == value.ProcessID && s.SessionReadyForLogonUserInputOffsetMSec <= 0))
                {
                    session.SessionReadyForLogonUserInputOffsetMSec = value.TimeStampRelativeMSec;
                }
            }
        }

        private void TriageShellCoreEvent(TraceEvent value)
        {
            if ((int) value.ID == 27231 || (int) value.ID == 9602)
            {
                foreach (
                    var session in
                        _sessionsPartial.Where(
                            s => s.ExplorerProcessId == value.ProcessID && s.ShellReadyOffsetMSec <= 0))
                {
                    session.ShellReadyOffsetMSec = value.TimeStampRelativeMSec;
                }
            }
            if ((int) value.ID == 60755)
            {
                foreach (
                    var session in
                        _sessionsPartial.Where(
                            s => s.ExplorerProcessId == value.ProcessID && string.IsNullOrWhiteSpace(s.SessionUserName))
                    )
                {
                    session.SessionUserName = value.PayloadNames.Contains("pszCurrentUserName")
                        ? (string) value.PayloadValue(1)
                        : "UnknownSessionUserName";
                }
            }
        }

        private void TriageProcessStartEvent(ProcessTraceData value)
        {
            if (_sessionsAlreadyRegistered.Contains(value.SessionID))
            {
                return;
            }
            if (_sessionsPartial.All(s => s.SessionId != value.SessionID))
            {
                _sessionsPartial.Add(new TraceModel.TerminalSession
                {
                    FirstProcessId = value.ProcessID,
                    FirstProcessName = value.ProcessName,
                    SessionId = value.SessionID,
                    SessionStartOffsetMSec = value.TimeStampRelativeMSec
                }
                    );
            }
            if (string.Equals(value.ProcessName, WinlogonProcessNameNoExtension, StringComparison.OrdinalIgnoreCase))
            {
                var session = _sessionsPartial.FirstOrDefault(s => s.SessionId == value.SessionID);
                if (session == null)
                {
                    throw new InvalidOperationException(
                        "Unexpected condition. Session should be recognized before seeing winlogon");
                }
                session.WinlogonPid = value.ProcessID;
            }
            if (string.Equals(value.ProcessName, ExplorerProcessNameNoExtension, StringComparison.OrdinalIgnoreCase))
            {
                var session = _sessionsPartial.FirstOrDefault(s => s.SessionId == value.SessionID);
                if (session == null)
                    throw new InvalidOperationException(
                        "Unexpected condition: session should be recognized before seeing explorer");
                if (session.ExplorerProcessId > 0) return;
                session.ExplorerProcessId = value.ProcessID;
                session.ShellStartOffsetMSec = value.TimeStampRelativeMSec;
            }
        }

        private void RegisterFinishedSessions(bool includeIncompleteSessions, TraceJob traceJob)
        {
            foreach (var session in _sessionsPartial.Where(s => IsSessionReadyToRegister(s, includeIncompleteSessions)))
            {
                _sessionsAlreadyRegistered.Add(session.SessionId);
                traceJob.PublishMeasurement(session);
            }
        }

        private bool IsSessionReadyToRegister(TraceModel.TerminalSession session, bool includeIncompleteSessions = false)
        {
            if (_sessionsAlreadyRegistered.Contains(session.SessionId)) return false;
            if (session.SessionId < 1) return true;
            if (session.WinlogonPid != 0 && session.SessionUserName != null && session.ShellReadyOffsetMSec > 0)
            {
                return true;
            }
            return includeIncompleteSessions;
        }
    }
}