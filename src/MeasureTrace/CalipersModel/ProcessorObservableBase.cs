// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

using System;
using System.Collections.Generic;
using MeasureTrace.TraceModel;

namespace MeasureTrace.CalipersModel
{
    public abstract class ProcessorObservableBase : IObservable<IMeasurement>
    {
        private readonly List<IObserver<IMeasurement>> _observers = new List<IObserver<IMeasurement>>();
        internal TraceJob TraceJob;

        public IDisposable Subscribe(IObserver<IMeasurement> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
            return new Unsubscriber(_observers, observer);
        }

        protected virtual void RegisterMeasurement(IMeasurement measurement)
        {
            if (measurement == null) throw new ArgumentNullException("measurement");
            foreach (var observer in _observers)
            {
                observer.OnNext(measurement);
            }
            TraceJob?.PublishMeasurement(measurement);
        }

        public void EndProcessing()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
            _observers.Clear();
        }

        protected sealed class Unsubscriber : IDisposable
        {
            private readonly IObserver<IMeasurement> _observer;
            private readonly List<IObserver<IMeasurement>> _observers;

            public Unsubscriber(List<IObserver<IMeasurement>> observers, IObserver<IMeasurement> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }
}