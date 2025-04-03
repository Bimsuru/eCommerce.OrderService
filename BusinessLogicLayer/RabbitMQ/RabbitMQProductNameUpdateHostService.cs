using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateHostService : IHostedService
{
    private readonly IRabbitMQConsumer _rabbitMQConsumer;

    public RabbitMQProductNameUpdateHostService(IRabbitMQConsumer rabbitMQConsumer)
    {
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // declare queue name
        string queueName = "orders.product.update.name.queue";

        // declare routingKey AS bindingKey
        string routingKey = "product.update.name";
        string messageActionName = "update";

        _rabbitMQConsumer.Consume(routingKey: routingKey, queueName: queueName, messageActionName: messageActionName);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQConsumer.Dispose();

        return Task.CompletedTask;
    }
}
