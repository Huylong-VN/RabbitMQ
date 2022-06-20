using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace _1.RabbitMQ.Producer
{
    public static partial class RabbitMQExtention
    {
        public class ScheduleContext : DbContext
        {
            public static readonly string? Schema = typeof(ScheduleContext).GetTypeInfo().Assembly.GetName().Name;

            public ScheduleContext(DbContextOptions<ScheduleContext> options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.HasDefaultSchema(Schema);
            }
        }
    }
}
