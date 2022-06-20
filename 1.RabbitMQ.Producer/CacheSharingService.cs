
public class CacheSharingService : ICronJob
{
    private readonly ILogger<CacheSharingService> _logger;
    private readonly IMessageBusClient _messageBus;

    public CacheSharingService(
        ILogger<CacheSharingService> logger,
        IMessageBusClient messageBus)
    {
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task<string> Run()
    {
        _logger.LogInformation("Starting job: {Job}", nameof(CacheSharingService));
        _messageBus.Publish("OrderLogService", new object());
        return await Task.FromResult($"Published event cache {nameof(CacheSharingService)}");
    }
}