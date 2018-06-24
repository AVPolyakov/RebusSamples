using System.Linq;
using Castle.DynamicProxy;
using Rebus.Bus;
using TimePrinter;

namespace Common
{
    public static class BusExtensions
    {
        public static TBusHandler Sender<TBusHandler>(this IBus bus) where TBusHandler : class
        {
            return ProxyGeneratorHolder.Generator
                .CreateInterfaceProxyWithoutTarget<TBusHandler>(new Interceptor(bus));
        }

        private class Interceptor : IInterceptor
        {
            private readonly IBus _bus;

            public Interceptor(IBus bus)
            {
                _bus = bus;
            }

            public void Intercept(IInvocation invocation)
            {
                invocation.ReturnValue = _bus.Send(invocation.Arguments.Single());
            }
        }
    }
}