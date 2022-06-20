using BackgroundRabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddHostedService<BackgroundLogService>();
builder.Services.AddRabbitMQ(new RabbitMQSettings()
{
    Broker = configuration.GetSection("RabbitMQ")["broker"],
    Connection = configuration.GetSection("RabbitMQ")["connection"],
    Password = configuration.GetSection("RabbitMQ")["password"],
    Port = int.Parse(configuration.GetSection("RabbitMQ")["port"]),
    RetryCount = int.Parse(configuration.GetSection("RabbitMQ")["retryCount"]),
    UserName = configuration.GetSection("RabbitMQ")["userName"]
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var app = builder.Build();


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
    Console.Out.WriteLine(message);
    await Task.CompletedTask;
};
channel.BasicConsume(queueName, autoAck: true, consumer: consumer);


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
