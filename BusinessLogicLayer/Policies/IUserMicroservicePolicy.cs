using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUserMicroservicePolicy
{
    AsyncPolicyWrap<HttpResponseMessage> GetCombinedPolicy();
}
