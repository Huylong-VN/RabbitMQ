
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();


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
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
