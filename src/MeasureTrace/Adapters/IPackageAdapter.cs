using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public interface IPackageAdapter
    {
        void PopulateTraceAttributesFromFileName(Trace trace, string filePath);
        void PopulateTraceAttributesFromPackageContents(Trace trace, string pathToUnzippedPackage);
    }
}