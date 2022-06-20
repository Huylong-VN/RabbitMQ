public interface IMessageBusClient
{
    void Publish(string channelName, object data);

    // Task PublishAsync(string channelName, object data);

    string Subscribe<T>(string channelName, Func<T, Task> func);

    // Task SubscribeAsync<T>(string channelName, Func<T, Task> func);

    string SubscribeWorkQueue<T> (string channelName, Func<T, Task> func);

    // Task SubscribeWorkQueueAsync<T>(string channelName, Func<T, Task> func);

    void UnSubscribe(string channel);

    // Task UnsubscribeAsync(string channel);
}
