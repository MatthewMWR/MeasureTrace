// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace MeasureTrace.Adapters
{
    internal static class Zip
    {
        internal static DirectoryInfo UnzipPackage(string zipPath, string outPath)
        {
            if (string.IsNullOrWhiteSpace(zipPath)) throw new ArgumentNullException(nameof(zipPath));
            if (!File.Exists(zipPath)) throw new FileNotFoundException("", zipPath);
            if (Directory.Exists(outPath)) Directory.Delete(outPath, true);
            var outDir = Directory.CreateDirectory(outPath);
            var flattenDupRootDirOnUnzipPattern = "^" + outDir.Name + "/";
            using (var inputZipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            {
                var archive = new ZipArchive(inputZipStream);
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrWhiteSpace(entry.Name)) continue;
                    var entryPathRelativeToArchive = Regex.Replace(entry.FullName, flattenDupRootDirOnUnzipPattern, "",
                        RegexOptions.IgnoreCase);
                    if (string.IsNullOrWhiteSpace(entryPathRelativeToArchive)) continue;
                    var fileOutPath = Path.Combine(outDir.FullName,
                        entryPathRelativeToArchive.TrimStart(Path.DirectorySeparatorChar));
                    var fileOutDirPath = Path.GetDirectoryName(fileOutPath);
                    if (fileOutDirPath == null) continue;
                    if (!Directory.Exists(fileOutDirPath)) Directory.CreateDirectory(fileOutDirPath);
                    entry.ExtractToFile(fileOutPath);
                }
            }
            return outDir;
        }

        internal static DirectoryInfo UnzipPackage(string zipPath)
        {
            var zipPathFileInfo = new FileInfo(zipPath);
            var outPath = zipPathFileInfo.FullName.TrimEnd(zipPathFileInfo.Extension.ToCharArray());
            return UnzipPackage(zipPathFileInfo.FullName, outPath);
        }
    }
}