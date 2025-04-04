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
        string queueName = "orders.product.update.queue";

        // // declare routingKey AS bindingKey
        // string routingKey = "product.update.*";

        // create headers
        var headers = new Dictionary<string, object>()
        {
            {"x-match", "all"},
            {"event", "product.update"},
            {"field", "name"},
            {"RowCount", 1}
        };

        string messageActionName = "update";

        _rabbitMQConsumer.Consume(routingKey: string.Empty, queueName: queueName, messageActionName: messageActionName, headers: headers);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQConsumer.Dispose();

        return Task.CompletedTask;
    }
}
