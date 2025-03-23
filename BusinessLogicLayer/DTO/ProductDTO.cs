namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record ProductDTO(
    Guid ProductID,
    string? ProductName,
    string? Category,
    double UnitPrice,
    int QuantityInStock
)
{
    public ProductDTO() : this(default, default, default, default, default)
    {
        
    }

}