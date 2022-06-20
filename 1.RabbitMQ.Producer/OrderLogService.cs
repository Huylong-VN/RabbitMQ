using RabbitMQ.Service;
using System.Diagnostics;

namespace _1.RabbitMQ.Producer
{
    public class OrderLogService : EventBusListenerService<object>
    {
        private bool _isProcessing;
        protected override bool Disabled => _isProcessing;
        protected override string Channel => nameof(OrderLogService);
        protected override bool IsWorkQueue => false;

        private readonly ILogger<OrderLogService> _logger;
        public OrderLogService(IServiceProvider serviceProvider, ILogger<OrderLogService> logger) : base(serviceProvider)
        {
            _logger = logger;
            _isProcessing = false;
        }


        protected override async Task Processing(object signal)
        {
            _isProcessing = true;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.LogWarning("start... ");
            stopwatch.Stop();
            _logger.LogInformation("job done. Elapsed time: {Time}",
                stopwatch.ElapsedMilliseconds.ToString());
            _isProcessing = false;
        }
    }
}
