using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace _1.RabbitMQ.Producer
{
    public class RabbitMQDocument : IRabbitMQDocument
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();

        public string Search { get; set; } = default!;

        public long SubId { get; set; }

        public Guid? CreatedBy { get; set; }

        public Guid? ModifiedBy { get; set; }

        public virtual void ModelCreating<T>(ModelBuilder modelBuilder, string schema) where T : RabbitMQDocument
        {
            var sequenceName = typeof(T).FullName.CreateMd5("Sequence");
            modelBuilder.HasSequence<int>(sequenceName);
            modelBuilder.Entity<T>().Property(o => o.SubId)
                .HasDefaultValueSql($"NEXT VALUE FOR [{schema}].{sequenceName}");
            modelBuilder.Entity<T>().HasIndex(x => x.SubId);
        }
    }
}
