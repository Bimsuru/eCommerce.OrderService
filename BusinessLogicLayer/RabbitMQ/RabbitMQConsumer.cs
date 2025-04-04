using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IConfiguration _configuration;
    private readonly RabbitMQConsumeServicesAction _rabbitMQConsumeServicesAction;

    public RabbitMQConsumer(IConfiguration configuration, ILogger<RabbitMQConsumer> logger, RabbitMQConsumeServicesAction rabbitMQConsumeServicesAction)
    {
        _configuration = configuration;
        _rabbitMQConsumeServicesAction = rabbitMQConsumeServicesAction;

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
    }
    public void Consume(string queueName, string routingKey, string messageActionName, Dictionary<string, object> headers)
    {

        // producer exchange name get in env 
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        // create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: headers); // argument: x-message-ttl | x-max-length | x-expired

        // bind the message into queue from exchnager
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

        // EventHandler
        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel); // this responsible for read message from the queue

        consumer.Received += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            // convert into json
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                // pass the specific action method into this message 
                if (messageActionName == "update")
                {
                    // consume new update product added into cache
                    await _rabbitMQConsumeServicesAction.ProductUpdateMessage(message: message);

                }
                else if (messageActionName == "delete")
                {
                    // consume deleted product remove from the cache
                    await _rabbitMQConsumeServicesAction.ProductDeleteMessage(message: message);
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
