using Microsoft.AspNetCore.Mvc.Testing;

namespace IceCreamTruck.WebApi.Tests;

/// <summary>
/// Custom WebApplicationFactory that allows for service replacement in tests.
/// </summary>
/// <typeparam name="TProgram">The program class of the web application.</typeparam>
public class CustomWebApplicationFactory<TProgram>(Action<IServiceCollection>? configureServices = null) : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly Action<IServiceCollection>? _configureServices = configureServices;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => _configureServices?.Invoke(services));
    }
}
