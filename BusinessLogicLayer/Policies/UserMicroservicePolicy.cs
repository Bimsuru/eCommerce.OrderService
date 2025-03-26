using Microsoft.Extensions.Logging;
using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UserMicroservicePolicy : IUserMicroservicePolicy
{
    private readonly ILogger<UserMicroservicePolicy> _logger;

    public UserMicroservicePolicy(ILogger<UserMicroservicePolicy> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetRetryPloicy()
    {
        IAsyncPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 5, // number of retries
                sleepDurationProvider: retryAttemp => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttemp)), // Delay betweem each retries
                onRetry: (outcome, timespan, retryAttemp, context) =>
                {
                    _logger.LogInformation($"Retry {retryAttemp} after {timespan.TotalSeconds} seconds");
                });
        return policy;
    }
}
