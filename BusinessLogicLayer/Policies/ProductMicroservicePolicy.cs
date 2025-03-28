
using System.Text;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductMicroservicePolicy : IProductMicroservicePolicy
{
    private readonly ILogger<ProductMicroservicePolicy> _logger;
    private readonly IPollyPolicies _pollyPolicies;

    public ProductMicroservicePolicy(ILogger<ProductMicroservicePolicy> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }
    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(async (context) =>
            {
                _logger.LogWarning("Fallback triggered: request failed, dummy data returning");

                ProductDTO product = new ProductDTO(
                    ProductID: Guid.Empty,
                    ProductName: "temporarily unavailable (fallback)",
                    Category: "temporarily unavailable (fallback)",
                    UnitPrice: 0,
                    QuantityInStock: 0
                );

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
                };

                return response;
            });

        return policy;
    }

    public AsyncPolicyWrap<HttpResponseMessage> GetCombinedPolicy()
    {
        var fallbackPolicy = GetFallbackPolicy();
        var bulkheadPolicy = _pollyPolicies.GetBulkheadIsolationPolicy(2,40);
        AsyncPolicyWrap<HttpResponseMessage> policies = Policy.WrapAsync<HttpResponseMessage>(fallbackPolicy, bulkheadPolicy);

        return policies;
    }
}
