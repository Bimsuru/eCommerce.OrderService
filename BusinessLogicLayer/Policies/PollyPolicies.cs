using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<UserMicroservicePolicy> _logger;

    public PollyPolicies(ILogger<UserMicroservicePolicy> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy(int maxParallelization, int maxQueuingActions)
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>
        (
         maxParallelization, // maximum number of concurrent actions that may be executing
         maxQueuingActions,  // waiting queue 40 requests
         onBulkheadRejectedAsync: async (context) =>
         {
             throw new BulkheadRejectedException("An action to call asynchronously, if the bulkhead rejects execution due to oversubscription");
         }
        );
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .CircuitBreakerAsync(
              handledEventsAllowedBeforeBreaking, // after (handledEventsAllowedBeforeBreaking: 3) number of retries enter the circuit breaker 
              durationOfBreak, // Delay betweem each retries
              onBreak: (outcome, timespan) =>
              {
                  _logger.LogInformation($"Circuit breaker opened for {timespan.Minutes} Minutes due to consecutive 3 failures. The subsequent requests will be blocked");
              },
              onReset: () =>
              {
                  _logger.LogInformation($"Circuit breacker closed. The subsequent requests will be allowed");

              }
            );

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .FallbackAsync(async (context) =>
            {
                _logger.LogWarning("Fallback triggered: request failed, dummy data returning");

                var response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
                return response;
            });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        IAsyncPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount, // number of retries
                sleepDurationProvider: retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)), // Delay betweem each retries -> avoid exponential backoff (port exhosting)
                onRetry: (outcome, timespan, retryAttemp, context) =>
                {
                    _logger.LogInformation($"Retry {retryAttemp} after {timespan.Seconds} seconds");
                });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);
        return policy;
    }
}
