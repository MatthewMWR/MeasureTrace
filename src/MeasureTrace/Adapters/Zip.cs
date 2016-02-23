// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.IO;
using System.IO.Compression;

namespace MeasureTrace.Adapters
{
    internal static class Zip
    {
        internal static DirectoryInfo UnzipPackage(string zipPath, string outPath)
        {
            if (string.IsNullOrWhiteSpace(zipPath)) throw new ArgumentNullException("zipPath");
            if (!File.Exists(zipPath)) throw new FileNotFoundException("", zipPath);
            if (Directory.Exists(outPath)) Directory.Delete(outPath,true);
            var outDir = Directory.CreateDirectory(outPath);
            //outDir = Directory.Exists(outPath) ? Directory.Delete().CreateDirectory(outPath) : new DirectoryInfo(outPath);
            using (var inputZipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            {
                var archive = new ZipArchive(inputZipStream);
                archive.ExtractToDirectory(outPath);
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