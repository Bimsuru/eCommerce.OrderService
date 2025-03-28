
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace BusinessLogicLayer.HttpClients;

public interface IProductsMicroserviceClient
{
    Task<ProductDTO?> GetProductAsync(Guid id);
}
