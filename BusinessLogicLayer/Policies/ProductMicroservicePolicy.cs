
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

    public AsyncPolicyWrap<HttpResponseMessage> GetCombinedPolicy()
    {
        var fallbackPolicy = _pollyPolicies.GetFallbackPolicy();
        var bulkheadPolicy = _pollyPolicies.GetBulkheadIsolationPolicy(2, 40);
        AsyncPolicyWrap<HttpResponseMessage> policies = Policy.WrapAsync<HttpResponseMessage>(fallbackPolicy, bulkheadPolicy);

        return policies;
    }
}
