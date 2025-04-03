
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQConsumer
{
    void Consume(string queueName, string routingKey, string messageActionName);
    void Dispose();
}
