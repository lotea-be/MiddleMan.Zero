using IceCreamTruck.Contracts;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetAsync(Guid orderId);
}