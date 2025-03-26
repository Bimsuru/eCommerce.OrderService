using System.Net;
using System.Net.Http.Json;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    public ProductsMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDTO?> GetProductAsync(Guid id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/products/{id}");

        // Check response IsSuccess or not
        if(!response.IsSuccessStatusCode)
        {
            if(response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            else if(response.StatusCode == HttpStatusCode.BadRequest)
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

        if(product == null)
        {
            return null;
        }

        return product;
    }
}
