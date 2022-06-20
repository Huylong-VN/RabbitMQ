using Microsoft.AspNetCore.Mvc;

namespace _1.RabbitMQ.Producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IMessageBusClient _messagePublisher;

        public OrdersController( IMessageBusClient messagePublisher)
        {
            _messagePublisher = messagePublisher;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(string? message)
        {
            _messagePublisher.Publish("OrderLogService", message);

            return Ok(message);
        }
    }
}
