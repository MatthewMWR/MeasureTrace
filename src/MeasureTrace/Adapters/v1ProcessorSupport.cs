// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using MeasureTrace.CalipersModel;

namespace MeasureTrace.Adapters
{
    public enum ProcessorTypeCollisionOption
    {
        ReplaceAnyExisting = 0,
        AddToAnyExisting,
        UseExistingIfFound
    }

    public static class V1ProcessorSupport
    {
        /// <summary>
        ///     Causes a processor to be added to the visible list of processors
        ///     on the TraceProcessJob
        ///     The instantiator remains reponsible for lifetime.
        ///     To avoid dispose responsibility use RegisterProcessorByType instead
        /// </summary>
        public static ProcessorBase RegisterProcessorInstance(this TraceJob traceJob, ProcessorBase processor,
            ProcessorTypeCollisionOption option)
        {
            if (processor.TraceJob == null) processor.TraceJob = traceJob;
            var processors = traceJob.V1ProcessorList;
            if (option == ProcessorTypeCollisionOption.ReplaceAnyExisting)
            {
                var toRemove = new List<Tuple<Type, ProcessorBase>>();
                foreach (var pt in processors.Where(pt => pt.Item1 == processor.GetType()))
                {
                    pt.Item2.Dispose();
                    toRemove.Add(pt);
                }
                foreach (var pt in toRemove)
                {
                    processors.Remove(pt);
                }
            }
            var remainingMatch = processors.FirstOrDefault(p => p.Item1 == processor.GetType());
            if (option == ProcessorTypeCollisionOption.UseExistingIfFound &&
                remainingMatch != null)
                return remainingMatch.Item2;
            processors.Add(new Tuple<Type, ProcessorBase>(processor.GetType(), processor));
            processor.Initialize(traceJob);
            return processor;
        }

        public static ProcessorBase RegisterProcessorInstance(this TraceJob traceJob, ProcessorBase processor)
        {
            return traceJob.RegisterProcessorInstance(processor, ProcessorTypeCollisionOption.ReplaceAnyExisting);
        }

        public static ProcessorBase RegisterProcessorByType<TProcessor>(this TraceJob traceJob,
            ProcessorTypeCollisionOption option)
            where TProcessor : ProcessorBase, new()
        {
            var existing = traceJob.GetRegisteredProcessors().OfType<TProcessor>().FirstOrDefault();
            if (existing != null && option == ProcessorTypeCollisionOption.UseExistingIfFound)
            {
                return existing;
            }
            var p = new TProcessor();
            traceJob.V1ProcessorsInternallyOwned.Add(p);
            return traceJob.RegisterProcessorInstance(p, option);
        }

        public static IEnumerable<ProcessorBase> GetRegisteredProcessors(this TraceJob traceJob)
        {
            return traceJob.V1ProcessorList.Select(p => p.Item2);
        }
    }
}