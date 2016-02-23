// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MeasureTrace.CalipersModel;

namespace MeasureTrace.Calipers
{
    public static class DiskIoDomainKnowledge
    {
        public const string ProgramFilesLabel = "%ProgramFiles%";
        public const string CommonFilesLabel = "%Common%";
        public const string UserProfilesRootLabel = "%UserProfiles%";
        public const string WinDirLabel = "%WinDir%";
        public const string ProgramDataLabel = "%ProgramData%";

        public static ICollection<Tuple<string, string>> NewDefaultReplacementsTable()
        {
            var l = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    ProgramFilesLabel),
                new Tuple<string, string>(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    ProgramFilesLabel),
                new Tuple<string, string>(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
                    CommonFilesLabel),
                new Tuple<string, string>(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86),
                    CommonFilesLabel),
                new Tuple<string, string>(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    ProgramDataLabel),
                new Tuple<string, string>(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows), WinDirLabel)
            };
            var profRootTokens =
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)
                    .Split(Path.DirectorySeparatorChar)
                    .Take(2);
            var profRootPath = string.Join(@"\", profRootTokens);
            l.Add(new Tuple<string, string>(profRootPath, UserProfilesRootLabel));

            var programW6432 = Environment.GetEnvironmentVariable("ProgramW6432");
            if (!string.IsNullOrWhiteSpace(programW6432))
            {
                l.Add(new Tuple<string, string>(programW6432, ProgramFilesLabel));
            }
            var commonW6432 = Environment.GetEnvironmentVariable("CommonProgramW6432");
            if (!string.IsNullOrWhiteSpace(commonW6432))
            {
                l.Add(new Tuple<string, string>(commonW6432, CommonFilesLabel));
            }
            return l;
        }

        public static string ReducePathEntropy(string originalPath, ICollection<Tuple<string, string>> replacementsTable,
            ReducePathEntropyOptions options)
        {
            var newValue = originalPath;

            foreach (var row in replacementsTable)
            {
                var startsWithRegExPattern = "^" + Regex.Escape(row.Item1);
                newValue = Regex.Replace(newValue, startsWithRegExPattern, row.Item2, RegexOptions.IgnoreCase);

                //if (!originalPath.StartsWith(row.Item1, StringComparison.OrdinalIgnoreCase)) continue;
                //newValue = originalPath.Replace(row.Item1.ToUpperInvariant(), row.Item2);
                //break;
            }
            var tokenizedPath = newValue.Split(Path.DirectorySeparatorChar);
            if (string.Equals(tokenizedPath[0], ProgramFilesLabel, StringComparison.OrdinalIgnoreCase)
                || string.Equals(tokenizedPath[0], CommonFilesLabel, StringComparison.OrdinalIgnoreCase)
                || string.Equals(tokenizedPath[0], ProgramDataLabel, StringComparison.OrdinalIgnoreCase))
            {
                var effectiveDepthLimit = options.DepthLimit;
                if (tokenizedPath.Length > 1 &&
                    string.Equals(tokenizedPath[1], "Microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    effectiveDepthLimit += options.DepthBoostSpecial;
                }
                newValue = string.Join(@"\", tokenizedPath.Take(effectiveDepthLimit));
            }
            else if (string.Equals(tokenizedPath[0], UserProfilesRootLabel, StringComparison.OrdinalIgnoreCase))
            {
                var effectiveDepthLimit = options.DepthLimit;
                if (tokenizedPath.Contains("microsoft", StringComparer.OrdinalIgnoreCase))
                {
                    effectiveDepthLimit = effectiveDepthLimit + options.DepthBoostSpecial;
                }
                var includedTokens = new List<string> {tokenizedPath[0]};
                includedTokens.AddRange(tokenizedPath.Skip(2).Take(effectiveDepthLimit));
                newValue = string.Join(@"\", includedTokens);
            }
            else if (string.Equals(tokenizedPath[0], WinDirLabel))
            {
                var effectiveDepthLimit = options.DepthLimit;
                if (tokenizedPath.Contains("wbem", StringComparer.OrdinalIgnoreCase))
                {
                    effectiveDepthLimit = 3;
                }
                else if (tokenizedPath.Contains("winevt", StringComparer.OrdinalIgnoreCase))
                {
                    effectiveDepthLimit = 4;
                }
                else if (tokenizedPath.Contains("config", StringComparer.OrdinalIgnoreCase))
                {
                    effectiveDepthLimit = 4;
                }
                else if (tokenizedPath.Length > 3 &&
                         string.Equals(tokenizedPath[1], "system32", StringComparison.OrdinalIgnoreCase))
                {
                    effectiveDepthLimit = 3;
                }
                newValue = string.Join(@"\", tokenizedPath.Take(effectiveDepthLimit));
            }
            else
            {
                newValue = string.Join(@"\", tokenizedPath.Take(options.DepthLimit));
            }

            newValue = Regex.Replace(newValue, @"[\dA-Fa-f]{4,}", "%N%");

            return newValue.Length > options.LengthLimit ? newValue.Substring(0, options.LengthLimit) : newValue;
        }

        internal static string ResolveDiskIoAttributionBucket(string processName, string filePath, string fileSummary,
            List<Tuple<string, string, string>> overrideList = null)
        {
            if (overrideList != null)
            {
                foreach (var row in overrideList)
                {
                    if (!string.IsNullOrWhiteSpace(row.Item1) &&
                        processName.StartsWith(row.Item1, StringComparison.OrdinalIgnoreCase))
                    {
                        return row.Item3;
                    }

                    if (!string.IsNullOrWhiteSpace(row.Item2) &&
                        filePath.StartsWith(row.Item2, StringComparison.OrdinalIgnoreCase) ||
                        filePath.EndsWith(row.Item2, StringComparison.OrdinalIgnoreCase))
                    {
                        return row.Item3;
                    }
                }
            }
            if (string.Equals(processName, "system", StringComparison.OrdinalIgnoreCase))
            {
                return filePath.StartsWith("unknown", StringComparison.OrdinalIgnoreCase)
                    ? "BdyBtOrUnknown"
                    : fileSummary;
            }
            if (string.Equals(processName, "svchost.exe", StringComparison.OrdinalIgnoreCase))
            {
                return fileSummary;
            }
            return processName;
        }
    }
}