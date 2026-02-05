using IceCreamTruck.Contracts;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default);
}