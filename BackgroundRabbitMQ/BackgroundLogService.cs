using RabbitMQ.Service;

namespace BackgroundRabbitMQ
{
    public class BackgroundLogService : EventBusListenerService<object>
    {
        public BackgroundLogService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override string Channel => "OrderLogService";

        protected override bool IsWorkQueue => false;
        public int Count { get; set; }

        protected override async Task Processing(object signal)
        {
            Count += 1;
            Console.Out.WriteLine($"BackgroundLogService running ...{Count}");
        }
    }
}
