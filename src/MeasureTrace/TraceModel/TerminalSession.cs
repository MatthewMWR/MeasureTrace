// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.TraceModel
{
    public class TerminalSession : MeasurementWithDuration, IMeasurement
    {
#pragma warning disable 169
        // dummy "Backing field" for EF compat with no-setter properties
        private double _logonCredentialEntryToShellReady;
#pragma warning restore 169
#pragma warning disable 169
        // dummy "Backing field" for EF compat with no-setter properties
        private double _measurementQuality;
#pragma warning restore 169
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }
        public int SessionId { get; set; }
        public int WinlogonPid { get; set; }
        public string SessionUserName { get; set; }
        public int FirstProcessId { get; set; }
        public string FirstProcessName { get; set; }
        public int ExplorerProcessId { get; set; }
        public double SessionStartOffsetMSec { get; set; }
        public double SessionReadyForLogonUserInputOffsetMSec { get; set; }
        public double LastAuthenticateUserStartOffsetMSec { get; set; }
        public double ShellStartOffsetMSec { get; set; }
        public double ShellReadyOffsetMSec { get; set; }

        public double LogonCredentialEntryToShellReady
        {
            get
            {
                var initialValue = ShellReadyOffsetMSec - LastAuthenticateUserStartOffsetMSec;
                return initialValue > 0 ? initialValue : -1;
            }
        }

        public Trace Trace { get; set; }
        //public int TraceId { get; set; }

        public override MeasurementQuality MeasurementQuality
        {
            get
            {
                if (SessionId < 1) return MeasurementQuality.DefaultUsable;
                if (SessionId > 0 && ShellReadyOffsetMSec > 0) return MeasurementQuality.DefaultUsable;
                return base.MeasurementQuality;
            }
        }
    }
}