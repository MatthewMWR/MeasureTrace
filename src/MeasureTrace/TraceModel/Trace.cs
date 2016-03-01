// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MeasureTrace.TraceModel
{
    public class Trace
    {
        private readonly ConcurrentBag<IMeasurement> _measurements = new ConcurrentBag<IMeasurement>();
        public IEnumerable<TraceAttribute> TraceAttributes => _measurements.OfType<TraceAttribute>();

        public int Id { get; set; }

        public string DataPathStable { get; set; }
        public string DataFileNameRelative { get; set; }
        public string DataPathDuringProcessing { get; set; }

        public string ComputerName { get; set; }
        public DateTime TraceSessionStart { get; set; }
        public DateTime TraceDataStart { get; set; }
        public DateTime TracePackageTime { get; set; }

        public void AddMeasurement(IMeasurement measurement)
        {
            measurement.Trace = this;
            _measurements.Add(measurement);
        }


        public IEnumerable<TMeasurement> GetMeasurements<TMeasurement>()
        {
            return _measurements.OfType<TMeasurement>().AsEnumerable();
        }

        public IEnumerable<IMeasurement> GetMeasurementsAll()
        {
            return _measurements.AsEnumerable();
        }

        public IEnumerable<TMeasurement> GetMeasurementsByTypeExample<TMeasurement>(TMeasurement exampleMeasurement)
        {
            return _measurements.OfType<TMeasurement>().AsEnumerable();
        }

        public IEnumerable<Type> GetPresentMeasurementTypes()
        {
            return _measurements.GroupBy(m => m.GetType()).Select(g => g.Key);
        }
    }
}