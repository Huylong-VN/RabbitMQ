using RabbitMQ.Client;

public static class EventBusRegistration
{
    public static void AddRabbitMQ(this IServiceCollection services, RabbitMQSettings rabbitMqSettings)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqSettings.Connection,
            Port = rabbitMqSettings.Port,
            DispatchConsumersAsync = true
        };

        if (!string.IsNullOrEmpty(rabbitMqSettings.UserName))
        {
            factory.UserName = rabbitMqSettings.UserName;
        }

        if (!string.IsNullOrEmpty(rabbitMqSettings.Password))
        {
            factory.Password = rabbitMqSettings.Password;
        }

        services.AddSingleton(rabbitMqSettings);
        services.AddSingleton<IConnectionFactory>(factory);
        services.AddSingleton<IMessageBusClient, MessageBusClient>();
    }
}