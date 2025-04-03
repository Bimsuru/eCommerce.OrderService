
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
using Microsoft.Extensions.Hosting;

namespace eCommerce.ordersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeleteHostService : IHostedService
{
    private readonly IRabbitMQConsumer _rabbitMQConsumer;

    public RabbitMQProductDeleteHostService(IRabbitMQConsumer rabbitMQConsumer)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        string routingKey = "product.delete.productid";
        string queueName = "order.product.delete.productid.queue";
        string messageActionName = "delete";
        
        _rabbitMQConsumer.Consume(routingKey: routingKey, queueName: queueName, messageActionName: messageActionName);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQConsumer.Dispose();

        return Task.CompletedTask;
    }
}
