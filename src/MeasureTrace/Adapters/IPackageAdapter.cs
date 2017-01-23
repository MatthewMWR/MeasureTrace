//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public interface IPackageAdapter
    {
        void PopulateTraceAttributesFromFileName(Trace trace, string filePath);
        void PopulateTraceAttributesFromPackageContents(Trace trace, string pathToUnzippedPackage);
    }
}