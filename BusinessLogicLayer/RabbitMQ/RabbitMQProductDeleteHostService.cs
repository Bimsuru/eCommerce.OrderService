
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
        // string routingKey = "product.#";
        string queueName = "order.product.delete.queue";
        string messageActionName = "delete";

        // create headers
        var headers = new Dictionary<string, object>()
        {
            {"x-match", "all"},
            {"event", "product.delete"},
            {"RowCount", "1"}
        };

        _rabbitMQConsumer.Consume(routingKey: string.Empty, queueName: queueName, messageActionName: messageActionName, headers: headers);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _rabbitMQConsumer.Dispose();

        return Task.CompletedTask;
    }
}
