using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public interface IPackageAdapter
    {
        void PopulateTraceAttributesFromFileName(Trace trace, string fileNameRelative);
        void PopulateTraceAttributesFromPackageContents(Trace trace, string pathToUnzippedPackage);
    }
}