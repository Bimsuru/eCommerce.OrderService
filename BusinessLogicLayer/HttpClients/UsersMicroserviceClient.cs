
using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient : IUsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDTO?> GetUserAsync(Guid id)
    {
        try
        {
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
                // if any request faliers
                else
                {
                    // throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    return new UserDTO
                    {
                        UserID = Guid.Empty,
                        Email = "temporarily unavailable",
                        PersonName = "temporarily unavailable",
                        Gender = "temporarily unavailable",
                    };
                }
            }

            // if its response is success no any erros
            var user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new ArgumentException("Invalid User Id");
            }

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
