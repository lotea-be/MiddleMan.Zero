using IceCreamTruck.Contracts;

namespace IceCreamTruck.Repositories;

internal class OrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];
    private readonly object _gate = new();

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _orders.Add(order);
        }

        return Task.CompletedTask;
    }

    public Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_orders.FirstOrDefault(o => o.Id == orderId));
        }
    }

    public Task<bool> CancelAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return Task.FromResult(false);
            }

            order.Status = OrderStatus.Cancelled;
            return Task.FromResult(true);
        }
    }
}
