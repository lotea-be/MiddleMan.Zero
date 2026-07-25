using IceCreamTruck;
using IceCreamTruck.WebApi.Endpoints;

#if NET9_0_OR_GREATER
using Scalar.AspNetCore;
#endif

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddIceCreamTruck();

#if NET9_0_OR_GREATER
// Add OpenAPI document generation (built into ASP.NET Core 9+)
builder.Services.AddOpenApi();
#endif

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
#if NET9_0_OR_GREATER
    // Serve the OpenAPI document at /openapi/v1.json and the Scalar API reference UI at /scalar/v1
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Ice Cream Truck API"));
#endif
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapFlavorEndpoints();

app.Run();

// Make Program class accessible to WebApplicationFactory in tests
public partial class Program { }
