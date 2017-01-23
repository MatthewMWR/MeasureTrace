//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public class BxrRPackageAdapter : IPackageAdapter
    {
        // Example: BxrR__S-ome_Machine__BOOT__2015-08-21_11-50-37__Z2.zip
        public const string BxrRFileNamePattern =
            @"^BxrR__(?'ComputerName'.+)__(?'TriggerName'[^.]+)__(?'DateTime'\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})";

        public const string BxrRDateTimeFormat = @"yyyy-MM-dd_HH-mm-ss";
        private const string BxrRSupplementalDataFileNamePattern = "BxrR*__SupplementalComputerInfo.xml";
        private const string BxrRKeyValuePattern = @"<S\sN\=""(\w+)"">([^<]+)</S>";

        public void PopulateTraceAttributesFromFileName(Trace trace, string filePath)
        {
            var fileNameRelative = Path.GetFileName(filePath);
            if (string.IsNullOrWhiteSpace(fileNameRelative)) throw new ArgumentNullException(nameof(fileNameRelative));
            var match = Regex.Match(fileNameRelative, BxrRFileNamePattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                trace.ComputerName = match.Groups["ComputerName"].Value;
                var tracestartDateTimeString = match.Groups["DateTime"].Value;
                trace.TracePackageTime = DateTime.ParseExact(tracestartDateTimeString, BxrRDateTimeFormat,
                    new DateTimeFormatInfo());
                trace.AddMeasurement(new TraceAttribute
                {
                    Name = "Trigger",
                    StringValue = match.Groups["TriggerName"].Value
                });
            }
        }

        public void PopulateTraceAttributesFromPackageContents(Trace trace, string pathToUnzippedPackage)
        {
            var supplementalFile =
                Directory.EnumerateFileSystemEntries(pathToUnzippedPackage, BxrRSupplementalDataFileNamePattern)
                    .FirstOrDefault();
            if (supplementalFile == null) return;
            foreach (var line in File.ReadLines(supplementalFile))
            {
                var matchInfo = Regex.Match(line, BxrRKeyValuePattern, RegexOptions.IgnoreCase);
                if (!matchInfo.Success) continue;
                var itemName = matchInfo.Groups[1].Value;
                var itemValue = matchInfo.Groups[2].Value;
                if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(itemValue)) continue;
                if (string.Equals(itemName, "ProductSKU", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(itemName, "ProcessorIDsMerged", StringComparison.OrdinalIgnoreCase)) continue;
                var attr = new TraceAttribute
                {
                    Name = itemName,
                    StringValue = itemValue
                };
                if (string.Equals(attr.Name, "OSInstallDateWMI", StringComparison.OrdinalIgnoreCase))
                    attr.DateTimeValue = ManagementDateTimeConverter.ToDateTime(attr.StringValue);
                if (string.Equals(attr.Name, "manufacturer", StringComparison.Ordinal))
                    attr.Name = "Manufacturer";
                trace.AddMeasurement(attr);
            }
        }
    }
}