
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public interface IUsersMicroserviceClient
{
    Task<UserDTO?> GetUserAsync(Guid id);
}
