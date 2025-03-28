using System.Reactive.Linq;
using UnchainexWallet.Services;

namespace UnchainexWallet.Fluent.Extensions;

public static class EventBusExtensions
{
	public static IObservable<TEvent> AsObservable<TEvent>(this EventBus eventBus) where TEvent : notnull
	{
		return Observable.Create<TEvent>(observer =>
		{
			var subscription = eventBus.Subscribe<TEvent>(eventItem =>
			{
				observer.OnNext(eventItem);
			});

			return subscription;
		});
	}
}
