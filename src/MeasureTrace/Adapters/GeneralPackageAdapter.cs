//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.IO;
using System.Linq;
using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public class GeneralPackageAdapter : IPackageAdapter
    {
        public void PopulateTraceAttributesFromFileName(Trace trace, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) throw new FileNotFoundException(filePath, filePath);
            trace.PackageFileNameFull = filePath;
            trace.PackageFileName = fileInfo.Name;
            trace.TracePackageTime =
                new[] {fileInfo.LastWriteTimeUtc, fileInfo.CreationTimeUtc}.OrderBy(t => t.ToFileTimeUtc())
                    .Last();
        }

        public void PopulateTraceAttributesFromPackageContents(Trace trace, string pathToUnzippedPackage)
        {
        }
    }
}