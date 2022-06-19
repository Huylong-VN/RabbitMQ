using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;


public class MessageBusClient : IMessageBusClient, IDisposable
{
    private readonly ILogger<MessageBusClient> _logger;
    private readonly int _retryCount;
    private readonly IConnection _connection;
    private readonly string _broker;
    private bool _disposed;

    private readonly ConcurrentDictionary<string, IModel> _channels;
    private readonly ConcurrentDictionary<IModel, List<string>> _consumerTags;

    public MessageBusClient(IConnectionFactory connectionFactory,
        ILogger<MessageBusClient> logger, RabbitMQSettings appSettings, int retryCount = 5)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _channels = new ConcurrentDictionary<string, IModel>();
        _consumerTags = new ConcurrentDictionary<IModel, List<string>>();
        _connection = connectionFactory.CreateConnection();
        _broker = appSettings?.Broker;
        _retryCount = retryCount;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection.Dispose();
    }

    public void Publish(string channelName, object message)
    {
        var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(_retryCount,
                retryAttempt => TimeSpan.Zero,
                (ex, time) => _logger.LogWarning(ex,
                    "{Time}: Could not publish channel: {ChannelName} after {TotalSeconds}s ({Message})",
                    DateTimeOffset.Now, channelName, time.TotalSeconds, ex.Message)
            );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var channel = CreateChannel("PublishChannel");
        policy.Execute(() =>
        {
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 1; // none-persistent

            _logger.LogInformation("{Time}: Publishing to RabbitMQ. Channel {ChannelName}", DateTimeOffset.Now,
                channelName);
            channel.BasicPublish(
                exchange: _broker,
                routingKey: $"{_broker}_{channelName}",
                mandatory: true,
                basicProperties: properties,
                body: body);
        });
    }

    public string Subscribe<T>(string channelName, Func<T, Task> action)
    {
        var routingKey = $"{_broker}_{channelName}";

        var channel = CreateChannel(routingKey);

        var queueName = channel.QueueDeclare().QueueName;

        channel.QueueBind(exchange: _broker, queue: queueName, routingKey: $"{_broker}_{channelName}");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            await action(Convert<T>(message));
        };

        return channel.BasicConsume(queueName, autoAck: true, consumer: consumer);
    }

    public string SubscribeWorkQueue<T>(string channelName, Func<T, Task> action)
    {
        var queueName = $"{_broker}_{channelName}";

        var channel = CreateChannel(queueName);

        channel.QueueDeclare(queue: queueName, exclusive: false);

        channel.QueueBind(exchange: _broker, queue: queueName, routingKey: queueName);

        channel.BasicQos(0, 1, true);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            await action(Convert<T>(message));
        };

        var tag = channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        _consumerTags.AddOrUpdate(channel, new List<string> { tag }, (_, value) =>
        {
            if (!value.Contains(tag))
                value.Add(tag);
            return value;
        });

        return tag;
    }

    public void UnSubscribe(string consumerId)
    {
        var channel = _consumerTags.FirstOrDefault(t => t.Value.Contains(consumerId)).Key;
        _consumerTags[channel].Remove(consumerId);
        channel.BasicCancel(consumerId);
    }

    private IModel CreateChannel(string key)
    {
        return _channels.GetOrAdd(key, _ =>
        {
            var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: _broker, ExchangeType.Direct);

            return channel;
        });
    }

    private static T Convert<T>(string msg)
    {
        return typeof(T) == typeof(string) ? (T)(object)msg : JsonSerializer.Deserialize<T>(msg) ?? default!;
    }
}

