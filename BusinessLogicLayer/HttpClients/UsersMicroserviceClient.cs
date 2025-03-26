
using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;

    public UsersMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UserDTO?> GetUserAsync(Guid id)
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
                return new UserDTO{
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
}
