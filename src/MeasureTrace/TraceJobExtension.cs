using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MeasureTrace.Adapters;
using MeasureTrace.CalipersModel;

namespace MeasureTrace
{
    public static class TraceJobExtension
    {
        public static void RegisterCalipersAllKnown(this TraceJob traceJob)
        {
            foreach (var ct in GetKnownCaliperTypes())
            {
                traceJob.RegisterCaliper((ICaliper) Activator.CreateInstance(ct));
            }
            foreach (var pt in GetKnownProcessorTypes())
            {
                traceJob.RegisterProcessorInstance((ProcessorBase) Activator.CreateInstance(pt),
                    ProcessorTypeCollisionOption.UseExistingIfFound, true);
            }
        }

        public static IEnumerable<Type> GetKnownCaliperTypes()
        {
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                try
                {
                    types.AddRange(assembly.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeof (ICaliper))));
                }
                catch (ReflectionTypeLoadException)
                {
                }
                catch (FileNotFoundException)
                {
                }
            }
            return types.AsEnumerable();
        }

        public static IEnumerable<Type> GetKnownProcessorTypes()
        {
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                try
                {
                    types.AddRange(assembly.GetExportedTypes().Where(t => t.IsSubclassOf(typeof (ProcessorBase))));
                }
                catch (ReflectionTypeLoadException)
                {
                }
                catch (FileNotFoundException)
                {
                }
            }
            return types.AsEnumerable();
        }
    }
}