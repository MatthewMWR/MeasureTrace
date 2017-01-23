//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Generic;
using System.Linq;
using SStatus = System.ServiceProcess.ServiceControllerStatus;
using TransitionX = MeasureTrace.TraceModel.ServiceTransitionTypeEx;

namespace MeasureTrace.Calipers
{
    public static class ServicesDomainKnowledge
    {
        private static readonly ICollection<Tuple<SStatus, SStatus, TransitionX>>
            ServicesTransitionReference = new List<Tuple<SStatus, SStatus, TransitionX>>
            {
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.StartPending, SStatus.Running, TransitionX.Start),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.StartPending, SStatus.ContinuePending,
                    TransitionX.StartIncomplete),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.StartPending, SStatus.PausePending,
                    TransitionX.StartIncomplete),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.StartPending, SStatus.PausePending,
                    TransitionX.StartIncomplete),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.StartPending, SStatus.StopPending,
                    TransitionX.StartIncomplete),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.StartPending, SStatus.Stopped,
                    TransitionX.StartIncomplete),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.Running, SStatus.StopPending, TransitionX.Stop),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.Running, SStatus.Stopped, TransitionX.Stop),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.Running, SStatus.ContinuePending, TransitionX.Continue),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.Running, SStatus.Paused, TransitionX.Pause),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.Running, SStatus.PausePending, TransitionX.Pause),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.Stopped, SStatus.Running, TransitionX.Start),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.StopPending, SStatus.Stopped, TransitionX.Stop),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.PausePending, SStatus.Paused, TransitionX.Pause),
                new Tuple<SStatus, SStatus, TransitionX>(SStatus.ContinuePending, SStatus.Running, TransitionX.Continue)
            };

        public static TransitionX MeasureServiceTranitionStatus(SStatus oldStatus,
            SStatus newStatus)
        {
            if (oldStatus == newStatus)
            {
                return TransitionX.None;
            }
            var matchingTuple =
                ServicesTransitionReference.Where(t => t.Item1 == oldStatus).FirstOrDefault(t => t.Item2 == newStatus);

            if (matchingTuple == null)
            {
                return TransitionX.Other;
            }
            return matchingTuple.Item3;
        }
    }
}