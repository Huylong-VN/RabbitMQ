namespace _1.RabbitMQ.Producer
{
    public interface IRabbitMQDocument
    {
        Guid Id { get; set; }

        public long SubId { get; set; }
    }
}
