
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQConsumer
{
    void Consume(string queueName, string routingKey, string messageActionName, Dictionary<string, object> headers);
    void Dispose();
}
