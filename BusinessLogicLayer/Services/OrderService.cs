using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServicesHelperMethod;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using MongoDB.Driver;


namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;

public class OrderService : IOrderService
{
    private readonly IMapper _mapper;
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly UsersMicroserviceClient _usersMicroserviceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient;
    private readonly ValidationHelper _validationHelper;

    public OrderService(IMapper mapper, IOrderRepository orderRepository, IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator, UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient, ValidationHelper validationHelper)
    {
        _mapper = mapper;
        _orderRepository = orderRepository;
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
        _validationHelper = validationHelper;
    }

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        var (userDTO, products, order) = await _validationHelper.ValidateOrderRequestAsync(
            requestDTO: orderAddRequest,
            orderValidator: _orderAddRequestValidator,
            orderItemvalidator: _orderItemAddRequestValidator,
            getUserId: req => req.UserID,
            getOrderItems: req => req.OrderItems!
        );

        // TotalPrice & TotalBill calculation
        CalculationOrderTotals(order);

        // seed data into db
        var addOrder = await _orderRepository.AddOrder(order);

        var res = MapOrderResponse(userDTO, products, addOrder!);
        return res;

    }

    public async Task<bool> DeleteOrder(Guid orderid)
    {
        // check whether the orderid exist or not
        var filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderid);
        var existingOrder = await _orderRepository.GetOrderByCondition(filter);

        if (existingOrder == null)
            return false;

        bool isDeleted = await _orderRepository.DeleteOrder(orderid);
        return isDeleted;
    }
    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        var order = await _orderRepository.GetOrderByCondition(filter);

        if (order == null)
            return null;

        // Mapping
        var orderResponse = _mapper.Map<OrderResponse>(order);

        // Get UserDTO and bind orderReponse
        var userDTO = await _usersMicroserviceClient.GetUserAsync(order!.UserID);

        if (userDTO != null)
        {
            _mapper.Map<UserDTO, OrderResponse>(userDTO, orderResponse);
        }

        foreach (var getOrderItem in orderResponse.OrderItems!)
        {
            // get the ProductDTO
            var productDTO = await _productsMicroserviceClient.GetProductAsync(getOrderItem.ProductID);

            if (productDTO == null)
                continue;

            _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, getOrderItem);
        }

        return orderResponse;
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        var orders = await _orderRepository.GetOrdersByCondition(filter);

        // Mapping
        IEnumerable<OrderResponse?> ordersResponse = _mapper.Map<IEnumerable<OrderResponse>>(orders);

        // productName and category model bindning
        if (ordersResponse != null)
        {
            foreach (var order in ordersResponse)
            {
                // Get UserDTO and bind orderReponse
                var userDTO = await _usersMicroserviceClient.GetUserAsync(order!.UserID);

                if (userDTO == null)
                    continue;

                _mapper.Map<UserDTO, OrderResponse>(userDTO, order);

                foreach (var getOneOrderItem in order!.OrderItems!)
                {
                    // get the ProductDTO
                    var productDTO = await _productsMicroserviceClient.GetProductAsync(getOneOrderItem.ProductID);

                    if (productDTO == null)
                        continue;

                    _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, getOneOrderItem);
                }
            }
        }
        return ordersResponse!.ToList();
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        var (userDTO, products, order) = await _validationHelper.ValidateOrderRequestAsync(
            requestDTO: orderUpdateRequest,
            orderValidator: _orderUpdateRequestValidator,
            orderItemvalidator: _orderItemUpdateRequestValidator,
            getUserId: req => req.UserID,
            getOrderItems: req => req.OrderItems!
        );

        // TotalPrice & TotalBill calculation
        CalculationOrderTotals(order);

        // seed data into db
        var UpdatedOrder = await _orderRepository.AddOrder(order);

        var res = MapOrderResponse(userDTO, products, UpdatedOrder!);
        return res;

    }

    private static void CalculationOrderTotals(Order order)
    {
        // TotalPrice & TotalBill calculation
        foreach (var orderItem in order.OrderItems)
        {
            orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        }
        order.TotalBill = order.OrderItems.Sum(temp => temp.TotalPrice);
    }

    private OrderResponse? MapOrderResponse(UserDTO user, List<ProductDTO> products, Order order)
    {
        // Map addOrder into orderResponse
        var orderResponse = _mapper.Map<OrderResponse>(order);

        //  validated user's name and email into orderresponse
        _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);

        if (orderResponse != null)
        {
            foreach (var resOrderItem in orderResponse.OrderItems!)
            {
                var productDTO = products.Where(temp => temp.ProductID == resOrderItem.ProductID).FirstOrDefault();

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO, resOrderItem);
            }
        }
        return orderResponse;

    }
    
}
