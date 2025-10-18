using IceCreamTruck.Repositories;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddIceCreamTruckServices(this IServiceCollection services)
    {
        services.AddSingleton<IOrderRepository, OrderRepository>();

        return services;
    }
}