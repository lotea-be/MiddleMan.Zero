using IceCreamTruck.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace IceCreamTruck;

public static class DependencyInjection
{
    public static IServiceCollection AddIceCreamTruck(this IServiceCollection services)
    {
        services.AddMiddleManZero();
        services.AddSingleton<IOrderRepository, OrderRepository>();
        return services;
    }
}