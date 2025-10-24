using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

internal class OrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];

    public Task AddAsync(Order order)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetAsync(Guid orderId)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == orderId));
    }
}