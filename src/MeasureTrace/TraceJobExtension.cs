using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

        public static TracePackageType ResolvePackageType(string filePathFull)
        {
            var fileInfo = new FileInfo(filePathFull);
            if (string.Equals(fileInfo.Extension, ".etl", StringComparison.OrdinalIgnoreCase))
            {
                return TracePackageType.GenericEtl;
            }
            else if (string.Equals(fileInfo.Extension, ".zip", StringComparison.OrdinalIgnoreCase))
            {
                if (Regex.IsMatch(fileInfo.Name, BxrRPackageAdapter.BxrRFileNamePattern, RegexOptions.IgnoreCase))
                    return TracePackageType.BxrRZip;
                else if (Regex.IsMatch(fileInfo.Name, CluePackageAdapter.IcuFileNamePattern, RegexOptions.IgnoreCase))
                    return TracePackageType.IcuZip;
                else return TracePackageType.GenericZip;
            }
            else
            {
                throw new InvalidOperationException("Expected .etl or .zip file");
            }
        }

        public static IPackageAdapter GetPackageAdapter(TracePackageType packageType)
        {
            if(packageType == TracePackageType.BxrRZip) return new BxrRPackageAdapter();
            if (packageType == TracePackageType.IcuZip) return new CluePackageAdapter();
            return new GeneralPackageAdapter();
        }
    }
}