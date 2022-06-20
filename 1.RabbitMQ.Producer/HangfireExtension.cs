using _1.RabbitMQ.Producer;
using Hangfire;
using Hangfire.SqlServer;
using HangfireBasicAuthenticationFilter;
using RabbitMQ.Service;
using System.Reflection;

public static class HangfireExtension
{
    public static void AddHangfireConfiguration(this IServiceCollection services, AppSettings appSettings,
        string scheduleSchema)
    {
        var connectionString = appSettings.ConnectionStrings.DefaultConnection;
        var storage = new SqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            SchemaName = scheduleSchema + "." + Environment.MachineName,
        });
        services.AddHangfire(x => x.UseStorage(storage));
        JobStorage.Current = storage;
        services.AddHangfireServer();
    }


    public static void UseJobs(this WebApplication app)
    {
        var appSettings = RabbitMQExtention.Configuration(app.Environment);
        using var scope = app.Services.CreateScope();

        var jobs = scope.ServiceProvider.GetServices<ICronJob>();

        var scheduleTasks = appSettings.ScheduledTasks.ToDictionary(scheduledTask => scheduledTask.Id);

        foreach (var recurringJob in jobs)
        {
            var name = recurringJob.GetType().Name;

            if (scheduleTasks.TryGetValue(name, out var exp))
            {
                RecurringJob.AddOrUpdate(name, () => recurringJob.Run(), exp.CronExpression);
            }
        }
    }

    public static void UseHangfire(this WebApplication app)
    {
        var appSettings = RabbitMQExtention.Configuration(app.Environment);
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "Hangfire Dashboard",
            Authorization = new[]
            {
                new HangfireCustomBasicAuthenticationFilter
                {
                    User = appSettings.Hangfire.UserName,
                    Pass = appSettings.Hangfire.Password
                }
            },
            IgnoreAntiforgeryToken = true
        });
    }
}