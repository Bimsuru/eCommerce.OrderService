
using System.Net.Http.Json;
using System.Text.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient : IUsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<UserDTO?> GetUserAsync(Guid id)
    {
        try
        {
            // create cacheKey 
            string? cacheKey = $"user:{id}";

            // user's data check in cache 
            var cacheUserRes = await _distributedCache.GetStringAsync(cacheKey);

            if (cacheUserRes != null)
            {
                var userDTO = JsonSerializer.Deserialize<UserDTO>(cacheUserRes);
                return userDTO;
            }

            var response = await _httpClient.GetAsync($"/api/v1/users/{id}");

            // IsSuccessStatusCode false then inner if block (400, 500)
            if (!response.IsSuccessStatusCode)
            {
                // If id is not found
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                // check if its 500 (ordermicroservice internal error) 
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
                }

                // fallback policy return 503 then add fallback data into this
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    return new UserDTO
                    {
                        UserID = Guid.Empty,
                        Email = "temporarily unavailable (fallback)",
                        PersonName = "temporarily unavailable (fallback)",
                        Gender = "temporarily unavailable (fallback)",
                    };
                }

                // if any request faliers
                else
                {
                    throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                }
            }

            // if its response is success no any erros
            var user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new ArgumentException("Invalid User Id");
            }

            // user convert into json object
            string? userJson = JsonSerializer.Serialize(user);

            // add DistributedCacheEntryOptions
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                                                        .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5))
                                                        .SetSlidingExpiration(TimeSpan.FromMinutes(3));


            // write userDTO into cache
            await _distributedCache.SetStringAsync(cacheKey, userJson, options);


            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker is open state, due to the success response back from the dependancy service, Returning dummy data.");
            return new UserDTO
            {
                UserID = Guid.Empty,
                Email = "temporarily unavailable (circuit breaker)",
                PersonName = "temporarily unavailable (circuit breaker)",
                Gender = "temporarily unavailable (circuit breaker)",
            };
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout occurred while fetching users data. Returnning dummy data.");
            return new UserDTO
            {
                UserID = Guid.Empty,
                Email = "temporarily unavailable (timeout)",
                PersonName = "temporarily unavailable (timeout)",
                Gender = "temporarily unavailable (timeout)",
            };
        }
    }
}
