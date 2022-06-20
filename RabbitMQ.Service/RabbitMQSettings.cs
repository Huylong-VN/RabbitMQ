public class RabbitMQSettings
{
    public string? Connection { get; set; }

    public int Port { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? Broker { get; set; }

    public int RetryCount { get; set; }
}