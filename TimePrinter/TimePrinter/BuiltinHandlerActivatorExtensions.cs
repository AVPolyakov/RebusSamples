using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Rebus.Activation;
using Rebus.Handlers;

namespace TimePrinter
{
    public static class BuiltinHandlerActivatorExtensions
    {
        public static void RegisterBusHandler<TBusHandlerImpl>(this BuiltinHandlerActivator @this, 
            Func<TBusHandlerImpl> func)
        {
            foreach (var busHandlerType in typeof(TBusHandlerImpl).GetInterfaces())
            foreach (var methodInfo in busHandlerType.GetMethods())
            {
                var parameterType = methodInfo.GetParameters().Single().ParameterType;
                CheckUniqueness(parameterType, methodInfo);
                _createMethod.MakeGenericMethod(parameterType, typeof(TBusHandlerImpl))
                    .Invoke(null, new object[] {@this, func, methodInfo});
            }
        }

        private static readonly MethodInfo _createMethod = typeof(BuiltinHandlerActivatorExtensions).GetMethod(nameof(Create));

        public static void Create<TRequest, TBusHandlerImpl>(BuiltinHandlerActivator activator,
            Func<TBusHandlerImpl> func, MethodInfo methodInfo)
        {
            activator.Register(() => (IHandleMessages<TRequest>) ProxyGeneratorHolder.Generator
                .CreateInterfaceProxyWithoutTarget(
                    typeof(IHandleMessages<>).MakeGenericType(typeof(TRequest)),
                    new Interceptor<TBusHandlerImpl>(func(), methodInfo)));
        }

        private class Interceptor<TBusHandlerImpl> : IInterceptor
        {
            private readonly TBusHandlerImpl _busHandlerImpl;
            private readonly MethodInfo _methodInfo;

            public Interceptor(TBusHandlerImpl busHandlerImpl, MethodInfo methodInfo)
            {
                _busHandlerImpl = busHandlerImpl;
                _methodInfo = methodInfo;
            }

            public void Intercept(IInvocation invocation)
            {
                invocation.ReturnValue = _methodInfo.Invoke(_busHandlerImpl, invocation.Arguments);
            }
        }

        private static readonly ConcurrentDictionary<Type, MethodInfo> _methodInfos = 
            new ConcurrentDictionary<Type, MethodInfo>();

        private static void CheckUniqueness(Type parameterType, MethodInfo methodInfo)
        {
            if (_methodInfos.TryGetValue(parameterType, out var value))
            {
                if (value != methodInfo) throw new InvalidOperationException();
            }
            else
            {
                _methodInfos.TryAdd(parameterType, methodInfo);
            }
        }
    }
}