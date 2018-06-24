using Common;
using Contracts;
using Rebus.Bus;

namespace TimePrinter
{
    public static class BusExtensions
    {
        public static IDepositBusHandler Deposit(this IBus bus) => bus.Sender<IDepositBusHandler>();
    }
}