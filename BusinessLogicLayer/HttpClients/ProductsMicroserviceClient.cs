using System.Net;
using System.Net.Http.Json;
using BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient : IProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductAsync(Guid id)
    {
        try
        {
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
