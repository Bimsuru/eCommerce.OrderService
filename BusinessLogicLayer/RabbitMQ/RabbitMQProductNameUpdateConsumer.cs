using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateConsumer : IRabbitMQProductNameUpdateConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;

    public RabbitMQProductNameUpdateConsumer(IConfiguration configuration, ILogger<RabbitMQProductNameUpdateConsumer> logger)
    {
        _configuration = configuration;

        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string port = _configuration["RabbitMQ_Port"]!;

        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _logger = logger;
    }
    public void Consume()
    {
        // declare queue name
        string queueName = "orders.product.update..name.queue";

        // declare routingKey AS bindingKey
        string routingKey = "product.update.name";

        // producer exchange name get in env 
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        // create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); // argument: x-message-ttl | x-max-length | x-expired

        // bind the message into queue from exchnager
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
        
        // EventHandler
        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel); // this responsible for read message from the queue

        consumer.Received += (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            // convert into json
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                // josn convert into productupdatename class objects
                var productUpdateNameMessage = JsonSerializer.Deserialize<ProductUpdateNameMessage>(message);

                if (productUpdateNameMessage != null)
                {
                    _logger.LogInformation($"Product name updated:{productUpdateNameMessage.ProductID}, New name:{productUpdateNameMessage.NewName}");

                }
            }

        };
        
        _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);


    }
    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
