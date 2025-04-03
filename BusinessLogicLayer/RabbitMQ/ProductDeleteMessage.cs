
namespace eCommerce.ordersMicroservice.BusinessLogicLayer.RabbitMQServices;


public record ProductDeleteMessage(Guid ProductID, string DeletedProductName);
