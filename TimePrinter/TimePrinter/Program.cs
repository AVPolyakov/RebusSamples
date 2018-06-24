using System;
using System.Threading.Tasks;
using System.Timers;
using Contracts;
using MessageHandlers.Host;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace TimePrinter
{
    class Program
    {
        const string InputQueueName = "my-app.input";

        static void Main()
        {
            using (var activator = new BuiltinHandlerActivator())
            using (var timer = new Timer())
            {
                activator.RegisterBusHandler(() => new DepositBusHandler());

                var bus = Configure.With(activator)
                    .Logging(l => l.None())
                    .Transport(t => t.UseMsmq(InputQueueName))
                    .Routing(r => r.TypeBased().Map<CreateAutopayRequest>(InputQueueName))
                    .Start();

                timer.Elapsed += delegate {
                    bus.Deposit().CreateAutopay(new CreateAutopayRequest {DepositId = 33}).Wait();
                };
                timer.Interval = 1000;
                timer.Start();

                Console.WriteLine("Press enter to quit");
                Console.ReadLine();
            }
        }
    }
}

namespace Contracts
{
    public interface IDepositBusHandler
    {
        Task CreateAutopay(CreateAutopayRequest request);
    }

    public class CreateAutopayRequest
    {
        public long DepositId { get; set; }
    }
}

namespace MessageHandlers.Host
{
    public class DepositBusHandler : IDepositBusHandler
    {
        public Task CreateAutopay(CreateAutopayRequest request)
        {
            Console.WriteLine($"DepositId = {request.DepositId}");
            return Task.CompletedTask;
        }
    }
}

//namespace Tests
//{
//    [Fact]
//    Task Test1()
//    {
//        var message = ...
//        await _busHost.Deposit().CreateAutopay(new CreateAutopayRequest {DepositId = 33});
//    }
//}