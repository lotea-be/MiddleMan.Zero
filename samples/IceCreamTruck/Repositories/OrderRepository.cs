using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

public class OrderRepository
{
    private readonly List<Order> _orders = [];

    public Task Add(Order order)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order[]> GetAll()
    {
        return Task.FromResult(_orders.ToArray());
    }

    public Task<Order?> GetById(Guid id)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    }
}