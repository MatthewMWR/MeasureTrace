//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Generic;

namespace MeasureTrace.CalipersModel
{
    public abstract class ProcessorBase : ProcessorObservableBase, IDisposable
    {
        private const string DefaultBaseExceptionMessage = "Error from TraceEvent parser";
        public Action PostTraceEventProcessing { get; set; }
        public Action PreTraceEventProcessing { get; set; }
        protected ICollection<IDisposable> Subscriptions { get; } = new List<IDisposable>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var subscription in Subscriptions)
                {
                    if (subscription != null)
                    {
                        subscription.Dispose();
                    }
                }
            }
        }

        //public ETWTraceEventSource EtwTraceEventSource { get; set; }
        public abstract void Initialize(TraceJob traceJob);

        public virtual void OnError(Exception e)
        {
            throw new InvalidOperationException(DefaultBaseExceptionMessage, e);
        }
    }
}