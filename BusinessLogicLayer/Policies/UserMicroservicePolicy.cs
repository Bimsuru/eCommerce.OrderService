using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;


namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UserMicroservicePolicy : IUserMicroservicePolicy
{
    private readonly ILogger<UserMicroservicePolicy> _logger;
    private readonly IPollyPolicies _pollyPolicies;

    public UserMicroservicePolicy(ILogger<UserMicroservicePolicy> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }
    public AsyncPolicyWrap<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
        var circuitBreakerPloicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromSeconds(20));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromMilliseconds(1500));
        var fallbackPolicy = _pollyPolicies.GetFallbackPolicy();

        AsyncPolicyWrap<HttpResponseMessage> policies = Policy.WrapAsync<HttpResponseMessage>(retryPolicy, circuitBreakerPloicy, timeoutPolicy, fallbackPolicy);
        
        return policies;
    }
}
