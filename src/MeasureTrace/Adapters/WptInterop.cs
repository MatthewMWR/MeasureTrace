// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MeasureTrace.TraceModel;
using Trace = System.Diagnostics.Trace;

namespace MeasureTrace.Adapters
{
    public static class WptInterop
    {
        public const string DiskIoTempOutputLabel = "DiskIO-Output";
        public const string DiskIoTempErrorLabel = "DiskIO-Error";
        public const int WptMagicNumberNullPostBootDuration = 9900;
        public const double WptMagicNumberUnboundDuration = 9223372034752.666;
        private const string XPerfExeName = "xperf.exe";
        private const string ArgumentsSeparator = " ";
        private const string FileNameMajorComponentSeparator = "__";
        private const string TempFileExtension = ".txt";
        private const string MinWptVersionString = "10.0.10240.16384";
        private const string WpaBootPhasesTableName = "BootphasesFlat";
        private const string WpaFullBootRegionsTableName = "FullBootFlat";
        private const string WpaProfileNameA = "MeasureTrace-ProfileA.wpaProfile";
        private const string WpaBootPhasesPreSessionInitLabel = "Pre Session Init";
        private const string WpaBootPhasesSessionInitLabel = "Session Init";
        private const string WpaBootPhasesWinlogonInitLabel = "Winlogon Init";
        private const string WpaBootPhasesExplorerInitLabel = "Explorer Init";
        private const string WpaBootPhasesPostBootLabel = "Post Boot";

        private const string WpaBootPhasesOutputFileName =
            "Boot_Phases_Summary_Table_" + WpaBootPhasesTableName + ".csv";

        private const string WpaFullBootRegionsOutputFileName =
            "Regions_of_Interest_" + WpaFullBootRegionsTableName + ".csv";

        private const string XPerfDiskIoParserString =
            @"^\W*(?'IOType'\w+),\s*(?'StartTime'\d+),\s*(?'EndTime'\d+),\s*(?'IoTime'\d+),\s*(?'DiskSvcTime'\d+),\s*(?'IOSize'[\d|x]+),\s*(?'ByteOffset'[\d|x|a-f]+),\s*(?'Pri'\d+),\s*(?'QDI'\d+),\s*(?'QDC'\d+),\s*(?'IBCB'\d+),\s*(?'IBCA'\d+),\s*(?'IACB'\d+),\s*(?'ProcessName'.[^\(]+)[\s\(]\s*(?'PID'\d+)\),\s*(?'Disk'\d+),\s*(?'FileName'[^,]+)\s*$";

        private static Process RunXPerf(string[] arguments, string pathToxPerfexe)
        {
            var processStartInfo = new ProcessStartInfo
            {
                Arguments = string.Join(ArgumentsSeparator, arguments),
                CreateNoWindow = true,
                FileName = pathToxPerfexe,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            var p = Process.Start(processStartInfo);
            if (p == null)
            {
                throw new FileNotFoundException();
            }
            return p;
        }

        private static Process RunXPerf(string[] arguments)
        {
            return RunXPerf(arguments, FindXPerfPath());
        }

        internal static string FindXPerfPath()
        {
            var minWptVersion = new Version(MinWptVersionString);
            var pathVar = Environment.GetEnvironmentVariable("Path");
            if (string.IsNullOrWhiteSpace(pathVar))
                throw new FileNotFoundException("path environment variable");
            var foundVersionPath = pathVar.Split(';')
                .Select(p => Path.Combine(p, XPerfExeName))
                .Where(p => File.Exists(p))
                .Select(p =>
                {
                    var fileVersion = FileVersionInfo.GetVersionInfo(p);
                    var version = new Version(fileVersion.FileMajorPart, fileVersion.FileMinorPart,
                        fileVersion.FileBuildPart, fileVersion.FilePrivatePart);
                    return new {FileVersion = fileVersion, Path = p, Version = version};
                })
                .OrderBy(vp => vp.Version)
                .LastOrDefault();

            if (foundVersionPath == null || string.IsNullOrWhiteSpace(foundVersionPath.Path))
                throw new FileNotFoundException("Could not find xperf.exe. Reinstall WPT 10, restart, and try again");
            if (foundVersionPath.Version < minWptVersion)
                throw new InvalidOperationException(
                    $"Minimum version of xperf/WPT not found. Expected version was {minWptVersion}. Found version was {foundVersionPath.Version}");
            return foundVersionPath.Path;
        }

        private static string NewProcessingTempFileName(string etlPath, string token)
        {
            var fileInfo = new FileInfo(etlPath);
            if (string.IsNullOrWhiteSpace(fileInfo.DirectoryName)) throw new InvalidOperationException();
            var newRelPath = fileInfo.Name + FileNameMajorComponentSeparator + token + TempFileExtension;
            return Path.Combine(fileInfo.DirectoryName, newRelPath);
        }

        private static List<string> NewXPerfArgumentsCore(string etlPath, bool tle, bool tti)
        {
            var args = new List<string>();
            args.Add("-i");
            args.Add('"' + etlPath + '"');
            args.Add("-target");
            args.Add("machine");
            if (tle) args.Add("-tle");
            if (tti) args.Add("-tti");
            return args;
        }

        internal static Dictionary<string, string> AddXperfDiskIoArguments(List<string> coreArguments, string etlPath)
        {
            var tempFiles = new Dictionary<string, string>();
            tempFiles.Add(DiskIoTempOutputLabel, NewProcessingTempFileName(etlPath, DiskIoTempOutputLabel));
            tempFiles.Add(DiskIoTempErrorLabel, NewProcessingTempFileName(etlPath, DiskIoTempErrorLabel));

            coreArguments.Add("-a");
            coreArguments.Add("diskio");
            coreArguments.Add("-detail");
            coreArguments.Add("-ao");
            coreArguments.Add('"' + tempFiles[DiskIoTempOutputLabel] + '"');
            coreArguments.Add("-ae");
            coreArguments.Add('"' + tempFiles[DiskIoTempErrorLabel] + '"');

            return tempFiles;
        }

        internal static Dictionary<string, string> RunXPerfAllProcessing(string etlPath)
        {
            var arguments = NewXPerfArgumentsCore(etlPath, true, true);
            var tempFiles = AddXperfDiskIoArguments(arguments, etlPath);
            var p = RunXPerf(arguments.ToArray());
            p.WaitForExit();
            return tempFiles;
        }

        private static Match ParseXPerfDiskIoLine(string line)
        {
            return Regex.Match(
                line,
                XPerfDiskIoParserString,
                RegexOptions.None
                );
        }

        private static DiskIo ConvertDiskIoLineMatchToDiskIoMeasurement(Match match)
        {
            //  Moving from all-in-one initializer to local variables to aid in validation and debugging of this
            //  unfortunate string parsing

            var ioTimeUSec = int.Parse(match.Groups["IoTime"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            var diskSvcTimeUSec = int.Parse(match.Groups["DiskSvcTime"].Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture);
            var ioType = match.Groups["IOType"].Value.Trim();
            var count = 1;
            var processName = match.Groups["ProcessName"].Value.Trim();
            var pathRaw = match.Groups["FileName"].Value.Trim();
            var bytes = int.Parse(match.Groups["IOSize"].Value.Substring(2), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture);

            var m = new DiskIo
            {
                IoTimeUSec = ioTimeUSec,
                DiskSvcTimeUSec = diskSvcTimeUSec,
                IoType = ioType,
                Count = count,
                ProcessName = processName,
                PathRaw = pathRaw,
                Bytes = bytes
            };
            return m;
        }

        public static IEnumerable<DiskIo> GetDiskIoMeasurementsFromXPerfOutput(string xPerfDiskIoOutputFile)
        {
            foreach (var line in File.ReadLines(xPerfDiskIoOutputFile))
            {
                var match = ParseXPerfDiskIoLine(line);
                if (match == null || match.Success == false)
                    continue;
                yield return ConvertDiskIoLineMatchToDiskIoMeasurement(match);
            }
        }

        private static string FindWpaExporterPath()
        {
            var xperfPath = FindXPerfPath();
            if (string.IsNullOrWhiteSpace(xperfPath)) throw new FileNotFoundException();
            var xperfFileInfo = new FileInfo(xperfPath);
            if (xperfFileInfo == null || xperfFileInfo.DirectoryName == null) throw new FileNotFoundException();
            var wpaExporterPath = Path.Combine(xperfFileInfo.DirectoryName, "wpaexporter.exe");
            if (string.IsNullOrWhiteSpace(wpaExporterPath) || !File.Exists(wpaExporterPath))
                throw new FileNotFoundException();
            return wpaExporterPath;
        }

        private static ICollection<string> NewWpaExporterArgs(string etlPath, string wpaProfilePath)
        {
            var etlFileInfo = new FileInfo(etlPath);
            var list = new List<string>();
            list.Add("-i");
            list.Add('"' + etlPath + '"');
            list.Add("-tle");
            list.Add("-tti");
            list.Add("-profile");
            list.Add('"' + wpaProfilePath + '"');
            list.Add("-prefix");
            list.Add(NewWpaOutputPrefix(etlPath, null));
            list.Add("-outputFolder");
            list.Add('"' + etlFileInfo.DirectoryName + '"');
            list.Add("-delimiter");
            list.Add('"' + @"\t" + '"');
            return list;
        }

        private static ICollection<string> NewWpaExporterArgs(string etlPath)
        {
            var expectedProfilePath = Path.Combine(Environment.CurrentDirectory, WpaProfileNameA);
            if (!File.Exists(expectedProfilePath)) throw new FileNotFoundException("wpaProfile");
            return NewWpaExporterArgs(etlPath, expectedProfilePath);
        }

        private static string NewWpaOutputPrefix(string etlPath, CultureInfo cultureInfo)
        {
            if (cultureInfo == null) cultureInfo = CultureInfo.CurrentCulture;
            var fileInfo = new FileInfo(etlPath);
            return string.Format(CultureInfo.InvariantCulture, "{0}%%LCID{1}",
                fileInfo.Name.TrimEnd(fileInfo.Extension.ToCharArray()).Replace(".", ""), cultureInfo.LCID);
        }

        private static IEnumerable<string> RunWpaExporter(string etlPath)
        {
            //  WPA (including wpaexporter) doesn't work correctly if there is no Documents folder
            //  This comes up in automation scenarios where the service account may not have a fully
            //  baked user profile folder structure
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
            var etlFileInfo = new FileInfo(etlPath);
            var etlDir = etlFileInfo.DirectoryName;
            if (string.IsNullOrWhiteSpace(etlDir)) throw new FileNotFoundException("etl path");
            var wpaExporterPath = FindWpaExporterPath();
            var args = NewWpaExporterArgs(etlPath);
            var startOptions = new ProcessStartInfo
            {
                Arguments = string.Join(" ", args.ToArray()),
                CreateNoWindow = true,
                FileName = wpaExporterPath,
                // Temp setting to false, see below TODO
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var p = Process.Start(startOptions);
            if (p == null) throw new InvalidOperationException("Failed to start WPA");
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            p.OutputDataReceived += (sendingProces, target) => outputBuilder.AppendLine(target.Data);
            p.ErrorDataReceived += (sendingProces, target) => errorBuilder.AppendLine(target.Data);
            p.WaitForExit(60000);
            Trace.TraceInformation($"WptInterop: WPA std out: {outputBuilder}");
            Trace.TraceInformation($"WptInterop: WPA std err: {errorBuilder}");
            var prefix = NewWpaOutputPrefix(etlPath, CultureInfo.CurrentCulture);
            yield return Path.Combine(etlDir, prefix + WpaBootPhasesOutputFileName);
            yield return Path.Combine(etlDir, prefix + WpaFullBootRegionsOutputFileName);
            //File.Delete(tempStdErrPath);
            //File.Delete(tempStdOutPath);
        }

        public static IEnumerable<BootPhase> GetWpaExporterBootPhases(string etlPath)
        {
            foreach (var outFile in RunWpaExporter(etlPath))
            {
                var outFileInfo = new FileInfo(outFile);
                if (!File.Exists(outFile)) continue;
                foreach (var row in File.ReadAllLines(outFile).Skip(1))
                {
                    var rowTokens = row.Split('\t');
                    if (outFileInfo.Name.EndsWith(WpaBootPhasesOutputFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        double? durationSeconds;
                        try
                        {
                            durationSeconds = Convert.ToDouble(rowTokens[2], CultureInfo.CurrentCulture.NumberFormat);
                        }
                        catch (OverflowException)
                        {
                            durationSeconds = null;
                        }
                        catch (FormatException)
                        {
                            durationSeconds = null;
                        }
                        yield return new BootPhase
                        {
                            DurationMSec = durationSeconds*1000,
                            BootPhaseObserver = BootPhaseObserver.WpaExporterBootPhases,
                            BootPhaseType = ResolveBootPhaseLabel(rowTokens[0])
                        };
                    }
                    else if (outFileInfo.Name.EndsWith(WpaFullBootRegionsOutputFileName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var durationSeconds = Convert.ToDouble(rowTokens[1], CultureInfo.CurrentCulture.NumberFormat);
                        var context = BootPhaseObserver.WpaExpoerterFullBootRegions;
                        switch (rowTokens[0].Trim())
                        {
                            // TODO FUTURE
                            // Potential globalization problem with default switch/case string matching. Consider change
                            // to safe compare mechanism
                            case @"Full Boot: /Boot Main Path: /Boot-PreSessionInit-Phase:":
                                yield return new BootPhase
                                {
                                    DurationMSec = durationSeconds*1000,
                                    BootPhaseObserver = context,
                                    BootPhaseType = BootPhaseType.PreSmss
                                };
                                break;
                            case @"Full Boot: /Boot Main Path: /Boot-SessionInit-Phase:":
                                yield return new BootPhase
                                {
                                    DurationMSec = durationSeconds*1000,
                                    BootPhaseObserver = context,
                                    BootPhaseType = BootPhaseType.Smss
                                };
                                break;
                            case @"Full Boot: /Boot Main Path: /Boot-Winlogon-Phase:":
                                yield return new BootPhase
                                {
                                    DurationMSec = durationSeconds*1000,
                                    BootPhaseObserver = context,
                                    BootPhaseType = BootPhaseType.Winlogon
                                };
                                break;
                            case @"Full Boot: /Boot Main Path: /Boot-ExplorerInit:":
                                yield return new BootPhase
                                {
                                    DurationMSec = durationSeconds*1000,
                                    BootPhaseObserver = context,
                                    BootPhaseType = BootPhaseType.Explorer
                                };
                                break;
                        }
                    }
                }
                outFileInfo.Delete();
            }
        }

        private static BootPhaseType ResolveBootPhaseLabel(string inputLabel)
        {
            if (string.Equals(inputLabel, WpaBootPhasesPreSessionInitLabel, StringComparison.OrdinalIgnoreCase))
                return BootPhaseType.PreSmss;
            if (string.Equals(inputLabel, WpaBootPhasesSessionInitLabel, StringComparison.OrdinalIgnoreCase))
                return BootPhaseType.Smss;
            if (string.Equals(inputLabel, WpaBootPhasesWinlogonInitLabel, StringComparison.OrdinalIgnoreCase))
                return BootPhaseType.Winlogon;
            if (string.Equals(inputLabel, WpaBootPhasesExplorerInitLabel, StringComparison.OrdinalIgnoreCase))
                return BootPhaseType.Explorer;
            if (string.Equals(inputLabel, WpaBootPhasesPostBootLabel, StringComparison.OrdinalIgnoreCase))
                return BootPhaseType.PostBoot;
            return BootPhaseType.None;
        }
    }
}