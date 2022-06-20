
using _1.RabbitMQ.Producer;
using static _1.RabbitMQ.Producer.RabbitMQExtention;

WebApplication.CreateBuilder(args).RabbitMQCore<ScheduleContext>(ScheduleContext.Schema,
    out var app, (service, appSettings) =>
    {
        service.AddSingleton(appSettings);
        service.AddRabbitMQ(appSettings.RabbitMQ);
        service.AddHangfireConfiguration(appSettings, ScheduleContext.Schema);
        service.AddScoped<ICronJob, CacheSharingService>();
        service.AddSingleton<IMessageBusClient, MessageBusClient>();
        service.AddHostedService<OrderLogService>();
    }, application =>
    {
        application.MapGet("/", () => $"{ScheduleContext.Schema} - Hello World!");
        application.UseJobs();
        application.UseHangfire();
    });

app.Run();

