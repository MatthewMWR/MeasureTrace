using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public class CluePackageAdapter : IPackageAdapter
    {
        public const string NameOfIcuMetaEmailReportToAttribute = "EmailReportTo";
        public const string NameOfIcuUserNoteAttribute = "UserInitiatedNote";
        private const string NameOfIcuMetaFile = "config.xml";
        private const string NameAlternateOfIcuMetaFile = "icu.xml";
        private const string LabelOfNoEmailReportToAddress = "NoEmailReportToAddress";
        private const string NameOfIcuUserNoteFile = "UserNote.txt";

        public const string IcuFileNamePattern =
            @"^(?'DateTime'\d{8}-\d{6})_(?'ComputerName'.+(?=_ICU)|.+(?=_Clue)|[^_]+)_(?'AgentName'ICU|Clue){0,1}[_]{0,1}(?'TriggerName'[^.]+)";

        public const string IcuDateTimeStringFormat = @"yyyyMMdd-HHmmss";

        public void PopulateTraceAttributesFromFileName(Trace trace, string fileNameRelative)
        {
            if (string.IsNullOrEmpty(fileNameRelative)) throw new ArgumentNullException(nameof(fileNameRelative));
            var match = Regex.Match(fileNameRelative, IcuFileNamePattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                trace.ComputerName = match.Groups["ComputerName"].Value;
                var tracestartDateTimeString = match.Groups["DateTime"].Value;
                trace.TracePackageTime = DateTime.ParseExact(tracestartDateTimeString, IcuDateTimeStringFormat,
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
            foreach (var emailAddress in GetIcuEmailReportToAddresses(pathToUnzippedPackage))
            {
                trace.AddMeasurement(new TraceAttribute
                {
                    Name = NameOfIcuMetaEmailReportToAttribute,
                    StringValue = emailAddress
                });
            }
            foreach (var userNote in GetIseUserInitiatedNote(pathToUnzippedPackage))
            {
                trace.AddMeasurement(new TraceAttribute
                {
                    Name = NameOfIcuUserNoteAttribute,
                    StringValue = userNote
                });
            }
        }

        private IEnumerable<string> GetIcuEmailReportToAddresses(string pathToIcuDataFolder)
        {
            var pathToIcuMetaFile = Path.Combine(pathToIcuDataFolder, NameOfIcuMetaFile);
            if (!File.Exists(pathToIcuMetaFile)) pathToIcuMetaFile = Path.Combine(pathToIcuDataFolder, NameAlternateOfIcuMetaFile);
            if (!File.Exists(pathToIcuMetaFile)) return new[] {string.Empty};
            var icuMeta = XElement.Load(pathToIcuMetaFile);
            var attributeValue = icuMeta.Attribute(XName.Get(NameOfIcuMetaEmailReportToAttribute, "")).Value;
            if (string.IsNullOrWhiteSpace(attributeValue))
                Logging.LogDebugMessage(LabelOfNoEmailReportToAddress);
            return attributeValue.Split(';').AsEnumerable();
        }

        private IEnumerable<string> GetIseUserInitiatedNote(string pathToIcuDataFolder)
        {
            var pathToFile = Path.Combine(pathToIcuDataFolder, NameOfIcuUserNoteFile);
            return !File.Exists(pathToFile) ? new[] {string.Empty} : File.ReadAllLines(pathToFile).AsEnumerable();
        }
    }
}