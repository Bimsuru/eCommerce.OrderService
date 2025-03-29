using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient : IProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<ProductDTO?> GetProductAsync(Guid id)
    {
        try
        {
            // Create cache key
            // cache store products like this --> key: "product:{id}" 
            // value: {"productName": "", "catogory":"",}
            string cacheKey = $"product: {id}";

            // Get product in cache
            string? cacheRes = await _distributedCache.GetStringAsync(cacheKey);

            // Not null then deseralized productcache josn object into productDTO
            if (cacheRes != null)
            {
                ProductDTO? productCache = JsonSerializer.Deserialize<ProductDTO>(cacheRes);
                return productCache;
            }

            var response = await _httpClient.GetAsync($"/api/v1/products/{id}");

            // Check response IsSuccess or not
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                }
                else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    // Fallback triggered: request failed, dummy data returning
                    
                    ProductDTO productFallbackRes = new ProductDTO(
                     ProductID: Guid.Empty,
                     ProductName: "temporarily unavailable (fallback)",
                     Category: "temporarily unavailable (fallback)",
                     UnitPrice: 0,
                     QuantityInStock: 0
                    );
                    return productFallbackRes;
                }
                else
                {
                    throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                }
            }

            // if IsSuccess
            var product = await response.Content.ReadFromJsonAsync<ProductDTO>();

            if (product == null)
            {
                return null;
            }

            // write the product into cache
            string? productJson = JsonSerializer.Serialize(product);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                                                        .SetAbsoluteExpiration(TimeSpan.FromMicroseconds(300))
                                                        .SetSlidingExpiration(TimeSpan.FromMicroseconds(100));

            await _distributedCache.SetStringAsync(productJson, cacheKey, options);

            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "The bulkhead queue are full and execution was rejected");

            return new ProductDTO(
                ProductID: Guid.Empty,
                ProductName: "temporarily unavailable (bulkhead isolation)",
                Category: "temporarily unavailable (bulkhead isolation)",
                UnitPrice: 0,
                QuantityInStock: 0
            );
        }
    }
}
