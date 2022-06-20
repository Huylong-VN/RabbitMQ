using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Service;
using System.Reflection;
using System.Text.Json.Serialization;

namespace _1.RabbitMQ.Producer
{
    public static partial class RabbitMQExtention
    {
        public static IServiceCollection RegisterController(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", x => x
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true));
            });
            services.AddControllers().AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
            return services;
        }
        public static void RabbitMQCore<TDbContext>(this WebApplicationBuilder builder, string schema,
                out WebApplication application, Action<IServiceCollection, AppSettings> actionServices = null,
                Action<WebApplication> actionApplication = null)
                where TDbContext : DbContext
        {
            var environment = builder.Environment;
            var services = builder.Services;
            var appSettings = Configuration(environment);

            services.AddDbContext<TDbContext>(options =>
            {
                var connectionString = appSettings.ConnectionStrings.DefaultConnection;
                options.UseSqlServer(connectionString, m =>
                {
                    m.MigrationsAssembly(schema);
                    m.MigrationsHistoryTable("__EFMigrationsHistory", schema);
                }
                );
            })
            ;

            services.AddHttpClient();
            services.AddSingleton(appSettings);
            services.RegisterController();
            services.AddMemoryCache();
            services.ConfigureResponseCaching();
            services.RegisterRedis(appSettings);
            actionServices?.Invoke(services, appSettings);

            application = builder.Build();
            application.UseStaticFiles();
            actionApplication?.Invoke(application);
            application.UseResponseCaching();
            application.UseCors("AllowAllOrigins");
            application.UseAuthentication();
            application.UseRouting();
            application.MapControllers();
            application.UseAuthorization();
        }
        public static AppSettings Configuration(IHostEnvironment environment)
        {
            var environmentName = environment.EnvironmentName;
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var appSettingsPath = Path.Combine(assemblyPath!, $"appsettings.{environmentName}.json");
            var appSettings = new AppSettings();
            new ConfigurationBuilder()
                .AddJsonFile(appSettingsPath)
                .Build().Bind(appSettings);
            return appSettings;
        }
       
        private static void ConfigureResponseCaching(this IServiceCollection services) => services.AddResponseCaching();
        private static Assembly[] GetAssemblies(params Assembly[] assemblies)
        {
            var assemblyEntries = assemblies?.Any() == true ? assemblies : new[] { Assembly.GetEntryAssembly() };

            IEnumerable<Assembly> _()
            {
                foreach (var asm in assemblyEntries)
                {
                    yield return asm;
                    foreach (var a in asm?.GetReferencedAssemblies()!)
                        yield return Assembly.Load(a);
                }
            }

            var getAssemblies = _()
                .Where(t => t != null && (t.FullName!.StartsWith("Service") || t.FullName!.StartsWith("Novanet")))
                .Distinct()
                .ToArray();
            return getAssemblies;
        }
        public static void RegisterRedis(this IServiceCollection services, AppSettings appSettings)
        {
            if (appSettings.GlobalCache != null)
                services.AddSingleton<IGlobalCacheService>(new RedisCacheService(appSettings.GlobalCache.Url));

            if (appSettings.RedisTracking != null)
                services.AddSingleton<IRedisCacheService>(new RedisCacheService(appSettings.RedisTracking.Url));
        }
    }
}
