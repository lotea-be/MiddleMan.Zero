using IceCreamTruck;
using IceCreamTruck.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddIceCreamTruck();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Ice Cream Truck API",
        Version = "v1",
        Description = "A sample API demonstrating MiddleMan.Zero with ASP.NET Core"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapFlavorEndpoints();

app.Run();

// Make Program class accessible to WebApplicationFactory in tests
public partial class Program { }
