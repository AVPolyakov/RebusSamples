using System;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Handlers;

namespace TimePrinter
{
	public static class BusExtensions
	{
		public static Task TypedSend<TMessage>(this IBus bus, TMessage message, Handle<TMessage> handle)
		{
			return bus.Send(message);
		}
	}

	public delegate Func<TMessage, Task> Handle<TMessage>(IHandleMessages<TMessage> handleMessages);
}