# Ice Cream Truck WebAPI

A sample ASP.NET Core Web API that demonstrates the usage of **MiddleMan.Zero** with ASP.NET Core MVC integration.

## Features

- RESTful API endpoints for ice cream orders
- Automatic conversion of `Result<T>` to appropriate HTTP responses using `MiddleMan.Zero.AspNetCore.Mvc`
- Swagger/OpenAPI documentation
- Dependency injection integration

## Running the Application

```bash
cd samples/IceCreamTruck.WebApi
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## API Endpoints

### POST /api/orders
Creates a new ice cream order.

**Request Body:**
```json
{
  "customerName": "John Doe",
  "iceCreams": [
    {
      "flavor": "Vanilla",
      "scoops": 2,
      "cone": true
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "John Doe",
  "iceCreams": [...],
  "status": "Pending",
  "orderDate": "2025-10-23T10:30:00Z"
}
```

### GET /api/orders/{id}
Retrieves an order by ID.

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerName": "John Doe",
  "iceCreams": [...],
  "status": "Completed",
  "orderDate": "2025-10-23T10:30:00Z"
}
```

**Response (404 Not Found):**
```json
{
  "status": "NotFound",
  "errors": ["Order not found"]
}
```

## How It Works

1. **Controllers** receive HTTP requests and use `IMiddleMan` to dispatch requests to handlers
2. **Handlers** process the business logic and return `Result<T>` objects
3. **ResultFilter** (from MiddleMan.Zero.AspNetCore.Mvc) automatically converts results to appropriate HTTP responses:
   - `Success` → 200 OK
   - `Created` → 201 Created
   - `NotFound` → 404 Not Found
   - `Invalid` → 400 Bad Request
   - `Error` → 500 Internal Server Error

## Project Structure

```
IceCreamTruck.WebApi/
├── Controllers/
│   └── OrdersController.cs          # API endpoints
├── Properties/
│   └── launchSettings.json          # Launch configuration
├── appsettings.json                 # Application settings
├── appsettings.Development.json     # Development settings
├── Program.cs                       # Application startup
└── IceCreamTruck.WebApi.csproj     # Project file
```

## Dependencies

- **IceCreamTruck**: Core business logic library
- **MiddleMan.Zero.AspNetCore.Mvc**: ASP.NET Core MVC integration for automatic result conversion
- **MiddleMan.Zero.DependencyInjection**: Dependency injection extensions
- **Swashbuckle.AspNetCore**: OpenAPI/Swagger documentation
