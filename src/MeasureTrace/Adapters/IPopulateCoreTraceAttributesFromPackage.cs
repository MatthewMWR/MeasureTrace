using MeasureTrace.TraceModel;

namespace MeasureTrace.Adapters
{
    public interface IPopulateCoreTraceAttributesFromPackage
    {
        void PopulateCoreTraceAttributesFromPackage(Trace trace, string dataPath);
    }
}