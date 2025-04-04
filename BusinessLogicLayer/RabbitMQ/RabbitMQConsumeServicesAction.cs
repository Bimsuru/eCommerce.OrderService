
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQConsumeServicesAction
{
    private readonly ILogger<RabbitMQConsumeServicesAction> _logger;
    private readonly IDistributedCache _cache;

    public RabbitMQConsumeServicesAction(ILogger<RabbitMQConsumeServicesAction> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }


    // product updated action from product microservice producer message
    public async Task ProductUpdateMessage(string message)
    {
        var productDTOMessage = JsonSerializer.Deserialize<ProductDTO>(message);
        _logger.LogInformation($"Product name updated:{productDTOMessage!.ProductID}, New name:{productDTOMessage.ProductName}");

        // create cashKey
        string productCacheKey = $"product: {productDTOMessage.ProductID}";

        // product json is not null create redis cache
        if (message != null)
        {
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                                                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

            await _cache.SetStringAsync(key: productCacheKey, value: message, options);
        }
    }

    public async Task ProductDeleteMessage(string message)
    {
        // josn convert into productDelete class objects
        var productMessage = JsonSerializer.Deserialize<ProductDTO>(message);
        _logger.LogInformation($"Product: {productMessage!.ProductID} is Deleted:, Deleted ProductName:{productMessage.ProductName}");

        // create cashKey
        string productCacheKey = $"product: {productMessage.ProductID}";

        if(message != null)
        {
            await _cache.RemoveAsync(productCacheKey);
        }
    }
}
