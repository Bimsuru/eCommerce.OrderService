using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUserMicroservicePolicy
{
    public IAsyncPolicy<HttpResponseMessage> GetRetryPloicy();
}
