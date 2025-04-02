
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public record ProductUpdateNameMessage(Guid ProductID, string NewName);
