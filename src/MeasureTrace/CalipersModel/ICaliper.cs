// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.CalipersModel
{
    public interface ICaliper
    {
        void RegisterFirstPass(TraceJob traceJob);
        void RegisterSecondPass(TraceJob traceJob);
    }
}