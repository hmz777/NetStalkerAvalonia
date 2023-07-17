using ReactiveUI;
using System;

namespace NetStalkerAvalonia.Core.Services.Implementations
{
	public interface IMessageBusService
	{
		public void SendMessage<T>(T message, string? contract = null);
		public IDisposable Listen<T>(Func<IObservable<T>, IDisposable> steps, string? contract = null);
		public IDisposable RegisterMessageSource<T>(IObservable<T> source, string? contract = null);
	}
}