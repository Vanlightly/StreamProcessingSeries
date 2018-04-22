using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace System.Reactive
{
    public static class RxExtensions
    {
        public static IDisposable SubscribeToLatest<T>(this IObservable<T> source, Action<T> action, IScheduler scheduler = null)
        {
            var sampler = new Subject<Unit>();
            scheduler = scheduler ?? Scheduler.Default;
            var p = source.Publish();
            var connection = p.Connect();

            var subscription = sampler.Select(x => p.Take(1))
                .Switch()
                .ObserveOn(scheduler)
                .Subscribe(l =>
                {
                    action(l);
                    sampler.OnNext(Unit.Default);
                });

            sampler.OnNext(Unit.Default);

            return new CompositeDisposable(connection, subscription);
        }
    }
}