using System;
using System.Globalization;
using System.Text.RegularExpressions;
using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public class BxrRInterop
    {
        // Example: BxrR__S-ome_Machine__BOOT__2015-08-21_11-50-37__Z2.zip
        public const string BxrRFileNamePattern =
            @"^BxrR__(?'ComputerName'.+)__(?'TriggerName'[^.]+)__(?'DateTime'\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})";
        public const string BxrRDateTimeFormat = @"yyyy-MM-dd_HH-mm-ss";

        public static void PopulateCoreTraceAttributesFromPackage(Trace trace)
        {
            if (string.IsNullOrWhiteSpace(trace.DataFileNameRelative)) throw new ApplicationException("Trace must have DataFileNameRelative");
            var match = Regex.Match(trace.DataFileNameRelative, BxrRFileNamePattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                trace.ComputerName = match.Groups["ComputerName"].Value;
                var tracestartDateTimeString = match.Groups["DateTime"].Value;
                trace.TracePackageTime = DateTime.ParseExact(tracestartDateTimeString, BxrRDateTimeFormat, new DateTimeFormatInfo());
                trace.AddMeasurement(new TraceAttribute()
                {
                    Name = "TriggerName",
                    StringValue = match.Groups["TriggerName"].Value
                });
            }

        }
    }
}