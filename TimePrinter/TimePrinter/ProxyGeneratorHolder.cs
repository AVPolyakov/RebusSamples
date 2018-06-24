using Castle.DynamicProxy;

namespace TimePrinter
{
    internal static class ProxyGeneratorHolder
    {
        public static readonly ProxyGenerator Generator = new ProxyGenerator();
    }
}