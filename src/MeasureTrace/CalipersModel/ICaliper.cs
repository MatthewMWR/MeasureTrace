//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Generic;

namespace MeasureTrace.CalipersModel
{
    public interface ICaliper
    {
        void RegisterFirstPass(TraceJob traceJob);
        void RegisterSecondPass(TraceJob traceJob);
        IEnumerable<Type> DependsOnCalipers { get; }
        
        
    }
}