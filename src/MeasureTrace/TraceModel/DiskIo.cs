﻿// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;

namespace MeasureTrace.TraceModel
{
    public class DiskIo : IMeasurement
    {
        public int Id { get; set; }
        public int MeasuredTraceId { get; set; }

        public int IoTimeMSecRounded
        {
            get { return Convert.ToInt32(Math.Round((double) IoTimeUSec/1000, 0)); }
        }

        public int DiskSvcTimeMSec
        {
            get { return Convert.ToInt32(Math.Round((double) DiskSvcTimeUSec/1000, 0)); }
        }

        public int IoTimeSecRounded
        {
            get
            {
                var rawSeconds = (double) IoTimeUSec/1000/1000;
                return Convert.ToInt32(Math.Round(rawSeconds, 0));
            }
        }

        public int DiskSvcTimeSecRounded
        {
            get
            {
                var rawSeconds = (double) DiskSvcTimeUSec/1000/1000;
                return Convert.ToInt32(Math.Round(rawSeconds, 0));
            }
        }

        public bool IsAggregate { get; set; }
        public int IoTimeUSec { get; set; }
        public int DiskSvcTimeUSec { get; set; }
        public string IoType { get; set; }
        public string PathRaw { get; set; }
        public string PathSummary { get; set; }
        public string ProcessName { get; set; }

        public string ProcessAndPathSummaryKey
        {
            get
            {
                var maxLength = 128;
                var string1 = ProcessName + "__" + PathSummary;
                return string1.Length < maxLength ? string1 : string1.Substring(0, maxLength);
            }
        }

        public int Count { get; set; }
        public int Bytes { get; set; }
        public string AttributionBucket { get; set; }

        public double MBytes
        {
            // ReSharper disable once PossibleLossOfFraction
            get { return Bytes/1024/1024; }
        }

        public Trace Trace { get; set; }
        public int TraceId { get; set; }

        public MeasurementQuality MeasurementQuality => MeasurementQuality.DefaultUsable;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}