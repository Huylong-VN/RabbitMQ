using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitMQ.Service
{
    public abstract class EventBusListenerService<T> : BackgroundService
    {
        private readonly ILogger<EventBusListenerService<T>> _logger;
        private readonly IMessageBusClient _eventBus;
        protected readonly IServiceProvider ServiceProvider;

        private string _consumerId;

        public EventBusListenerService(IServiceProvider serviceProvider)
        {
            _consumerId = string.Empty;
            ServiceProvider = serviceProvider.CreateScope().ServiceProvider;
            _logger = ServiceProvider.GetRequiredService<ILogger<EventBusListenerService<T>>>();
            _eventBus = serviceProvider.GetRequiredService<IMessageBusClient>();
        }

        protected abstract string Channel { get; }

        protected virtual bool Disabled { get; }

        protected abstract bool IsWorkQueue { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            async Task Callback(T signal)
            {
                try
                {
                    if (Disabled) return;
                    await BeforeProcessing(signal);
                    await Processing(signal);
                    await AfterProcessed(signal);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{GetType().Name} EXCEPTION");
                }
            }

            _consumerId = IsWorkQueue
                ? _eventBus.SubscribeWorkQueue<T>(Channel, async (signal) => await Callback(signal))
                : _eventBus.Subscribe<T>(Channel, async (signal) => await Callback(signal));

            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_consumerId))
            {
                _eventBus.UnSubscribe(_consumerId);
            }

            return base.StopAsync(cancellationToken);
        }

        protected abstract Task Processing(T signal);

        protected virtual async Task BeforeProcessing(T signal)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task AfterProcessed(T signal)
        {
            await Task.CompletedTask;
        }
    }
}