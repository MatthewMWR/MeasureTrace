using System.Diagnostics;

namespace MeasureTrace
{
    public class Logging
    {
        public const string MtDebugMessagePrefix = "MtLogDebugMessage";
        //  Whereas MeasureTrace core is meant to be portable and flexible I haven't
        //  taken a dependency on any particular logging framework
        //  Instead we log simple string to the built-in System.Diagnostics.Trace. Whatever is calling MeasureTrace can easily subscribe
        //  via Trace.Listeners and forward to preferred logging framework (e.g., MeasureTraceAutomation forwards to EventSource style logging)
        internal static void LogDebugMessage(string message)
        {
            Trace.TraceInformation($"{MtDebugMessagePrefix}: {message}");
        }
    }
}