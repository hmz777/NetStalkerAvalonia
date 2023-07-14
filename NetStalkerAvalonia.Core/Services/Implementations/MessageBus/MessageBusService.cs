using System;

namespace NetStalkerAvalonia.Core.Services.Implementations.MessageBus
{
	public class MessageBusService : IMessageBusService
	{
		public IDisposable Listen<T>(Func<IObservable<T>, IDisposable> steps, string? contract = null)
		{
			return steps(ReactiveUI.MessageBus.Current.Listen<T>(contract));
		}

		public IDisposable RegisterMessageSource<T>(IObservable<T> source, string? contract = null)
		{
			return ReactiveUI.MessageBus.Current.RegisterMessageSource<T>(source, contract);
		}

		public void SendMessage<T>(T message, string? contract = null)
		{
			ReactiveUI.MessageBus.Current.SendMessage<T>(message, contract);
		}
	}
}