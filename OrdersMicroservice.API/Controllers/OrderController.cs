
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroService.OrdersMicroservice.API.Controllers;

[Route("api/v1/orders")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderResponse?>>> GetAllOrders(Guid? productid, DateTime? orderDate, Guid? userid)
    {
        var filter = Builders<Order>.Filter.Empty;

        if (productid.HasValue)
        {
            filter &= Builders<Order>.Filter.ElemMatch(o => o.OrderItems,
                Builders<OrderItem>.Filter.Eq(oi => oi.ProductID, productid.Value));
        }
        if (userid.HasValue)
        {
            filter = Builders<Order>.Filter.Eq(oi => oi.UserID, userid.Value);
        }

        if (orderDate.HasValue)
        {
            // Consider date range if time component isn't important
            var startDate = orderDate.Value.Date;
            var endDate = startDate.AddDays(1);
            filter &= Builders<Order>.Filter.Gte(o => o.OrderDate, startDate) &
                      Builders<Order>.Filter.Lt(o => o.OrderDate, endDate);
        }

        var orders = await _orderService.GetOrdersByCondition(filter);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        var filterid = Builders<Order>.Filter.Eq(temp => temp.OrderID, id);
        var order = await _orderService.GetOrderByCondition(filterid);

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> PostOrder(OrderAddRequest orderAddRequest)
    {

        if (orderAddRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        OrderResponse? orderResponse = await _orderService.AddOrder(orderAddRequest);

        if (orderResponse == null)
        {
            return Problem("Error in adding product");
        }
        return Created($"api/v1/orders/{orderResponse?.OrderID}", orderResponse);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<OrderResponse>> PutOrder(Guid id, OrderUpdateRequest orderUpdateRequest)
    {

        if (orderUpdateRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        if (id != orderUpdateRequest.OrderID)
        {
            return BadRequest("OrderID in the URL doesn't match with the OrderID in the Request body");
        }
        var orderResponse = await _orderService.UpdateOrder(orderUpdateRequest);

        if (orderResponse == null)
        {
            return Problem("Error in adding product");
        }
        return Ok(orderResponse);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteOrder(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid order ID");
        }

        bool isDeleted = await _orderService.DeleteOrder(id);

        if (!isDeleted)
        {
            return Problem("Error in adding product");
        }

        return Ok(isDeleted);
    }

}
