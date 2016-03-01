//// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

//using System;
//using System.CodeDom;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Xml.Linq;
//using MeasureTrace.TraceModel;


//namespace MeasureTrace.Adapters
//{
//    public class IcuInterop
//    {
//        private const string NameOfIcuMetaEmailReportTo = "EmailReportTo";
//        private const string NameOfIcuMetaFile = "ICU.xml";
//        private const string LabelOfNoEmailReportToAddress = "NoEmailReportToAddress";
//        private const string NameOfIcuUserNoteFile = "UserNote.txt";
//        public const string IcuFileNamePattern = @"^(?'DateTime'\d{8}-\d{6})_(?'ComputerName'.+(?=_ICU)|.+(?=_Clue)|[^_]+)_(?'AgentName'ICU|Clue){0,1}[_]{0,1}(?'TriggerName'[^.]+)";
//        public const string IcuDateTimeStringFormat = @"yyyyMMdd-HHmmss";
//        public const string IcuNameOfTriggerAttribute = @"IcuTrigger";

//        public static IEnumerable<string> GetIcuEmailReportToAddresses(string pathToIcuDataFolder)
//        {
//            var pathToIcuMetaFile = Path.Combine(pathToIcuDataFolder, NameOfIcuMetaFile);
//            var icuMeta = XElement.Load(pathToIcuMetaFile);
//            var attributeValue = icuMeta.Attribute(XName.Get(NameOfIcuMetaEmailReportTo, "")).Value;
//            if (string.IsNullOrWhiteSpace(attributeValue)) throw new ApplicationException(LabelOfNoEmailReportToAddress);
//            return attributeValue.Split(';').AsEnumerable();
//        }

//        public static IEnumerable<string> GetIseUserInitiatedNote(string pathToIcuDataFolder)
//        {
//            var pathToFile = Path.Combine(pathToIcuDataFolder, NameOfIcuUserNoteFile);
//            return !File.Exists(pathToFile) ? new []{string.Empty} : File.ReadAllLines(pathToFile).AsEnumerable();
//        }

//        public static void PopulateCoreTraceAttributesFromPackage(Trace trace)
//        {
//            if(string.IsNullOrWhiteSpace(trace.DataFileNameRelative)) throw new ApplicationException("Trace must have DataFileNameRelative");
//            var match = Regex.Match(trace.DataFileNameRelative, IcuFileNamePattern, RegexOptions.IgnoreCase);
//            if (match.Success)
//            {
//                trace.ComputerName = match.Groups["ComputerName"].Value;
//                var tracestartDateTimeString = match.Groups["DateTime"].Value;
//                trace.TracePackageTime = DateTime.ParseExact(tracestartDateTimeString, IcuDateTimeStringFormat, new DateTimeFormatInfo());
//                trace.AddMeasurement(new TraceAttribute()
//                {
//                    Name = IcuNameOfTriggerAttribute,
//                    StringValue = match.Groups["TriggerName"].Value
//                });
//            }

//        }
//    }
//}

