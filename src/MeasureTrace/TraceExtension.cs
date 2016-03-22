using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MeasureTrace.TraceModel;

namespace MeasureTrace
{
    public static class TraceExtension
    {
        public static IEnumerable<Type> GetKnownMeasurementTypes(this Trace trace)
        {
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                try
                {
                    types.AddRange(
                        assembly.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeof (IMeasurement))));
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