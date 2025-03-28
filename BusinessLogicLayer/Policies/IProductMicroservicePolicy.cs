using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IProductMicroservicePolicy
{
    AsyncPolicyWrap<HttpResponseMessage> GetCombinedPolicy();
}
