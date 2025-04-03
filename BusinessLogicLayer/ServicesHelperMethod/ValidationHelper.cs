using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServicesHelperMethod;

public class ValidationHelper
{
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    private readonly IMapper _mapper;

    public ValidationHelper(UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient, IMapper mapper)
    {
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
        _mapper = mapper;
    }

    public async Task<(UserDTO userDTO, List<ProductDTO> productDTOs, Order order)> ValidateOrderRequestAsync<TRequest, TItem>(
        TRequest requestDTO,
        IValidator<TRequest> orderValidator,
        IValidator<TItem> orderItemvalidator,
        Func<TRequest, Guid> getUserId,
        Func<TRequest, IEnumerable<TItem>> getOrderItems)
    {

        // Check request is null or not
        if (requestDTO == null)
        {
            throw new ArgumentNullException(nameof(requestDTO));
        }

        // FluentValidation in orderAddRequest
        ValidationResult orderValidationResult = await orderValidator.ValidateAsync(requestDTO);

        // check the validation result 
        if (!orderValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        // Add logic for checking if UserID exists in Users microservice
        var user = await _usersMicroserviceClient.GetUserAsync(getUserId(requestDTO));

        if (user == null)
        {
            throw new ArgumentException("Invalid User id");
        }

        List<ProductDTO> products = new List<ProductDTO>();

        // Validate order items using Fluent Validation
        foreach (var orderItem in getOrderItems(requestDTO))
        {
            var orderItemValidationResult = await orderItemvalidator.ValidateAsync(orderItem);

            // Add logic for checking if productID exists in product microservice
            var product = await _productsMicroserviceClient.GetProductAsync((orderItem! as dynamic).ProductID);

            if (product == null)
            {
                throw new ArgumentException("Invalid product id");
            }
            products.Add(product);
            if (!orderItemValidationResult.IsValid)
            {
                string erros = string.Join(", ", orderItemValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(erros);
            }
        }

        // mapping order from the OrderAddRequest
        Order order = _mapper.Map<Order>(requestDTO);

        return (user, products, order);
    }
}
