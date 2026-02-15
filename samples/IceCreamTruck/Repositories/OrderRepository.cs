using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

internal class OrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == orderId));
    }
}
