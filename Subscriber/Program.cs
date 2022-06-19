using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


// Khởi tạo connection tới rabbitmq
var factory = new ConnectionFactory
{
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};
using var connection = factory.CreateConnection();

// tên channel
var channelName = "testQueue";
var routingKey = $"rabbitmq_{channelName}";

var channel = connection.CreateModel();

channel.ExchangeDeclare(exchange: "rabbitmq", ExchangeType.Direct);

var queueName = channel.QueueDeclare().QueueName;

channel.QueueBind(exchange: "rabbitmq", queue: queueName, routingKey: $"rabbitmq_{channelName}");

var consumer = new EventingBasicConsumer(channel);

consumer.Received += async (_, eventArgs) =>
{
    var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
    Console.WriteLine(message);
    await Task.CompletedTask;
};

channel.BasicConsume(queueName, autoAck: true, consumer: consumer);

Console.ReadLine();