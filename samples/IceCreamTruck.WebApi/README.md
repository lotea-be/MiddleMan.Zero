# Ice Cream Truck WebAPI

A sample ASP.NET Core Web API that demonstrates the usage of **MiddleMan.Zero** with ASP.NET Core MVC integration (controllers) and Minimal APIs.

## Features

- RESTful API endpoints for ice cream orders and flavors
- Automatic conversion of `Result<T>` to appropriate HTTP responses using `MiddleMan.Zero.AspNetCore.Mvc` and `MiddleMan.Zero.AspNetCore.Http`
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
  "items": [
    { "flavor": "Vanilla", "scoops": 2, "price": 5.00 }
  ]
}
```

**Responses:** `200 OK` (order ID), `400 Bad Request`

---

### GET /api/orders/{id}
Retrieves an order by ID.

**Responses:** `200 OK` (order object), `400 Bad Request`, `404 Not Found`

---

### DELETE /api/orders/{id}
Cancels an existing order. Requires the `X-Admin-User: true` header.

**Responses:**
- `200 OK` — order cancelled
- `400 Bad Request` — invalid order ID
- `403 Forbidden` — caller is not an admin (`X-Admin-User: false` or missing)
- `404 Not Found` — order does not exist

---

### GET /flavors
Lists all available ice cream flavors.

**Responses:** `200 OK` (flavor array)

---

### POST /flavors
Adds a new ice cream flavor.

**Request Body:**
```json
{ "flavorName": "Mint Chocolate Chip" }
```

**Responses:** `200 OK`, `400 Bad Request`

## How It Works

1. **Controllers / Minimal API endpoints** receive HTTP requests and inject the appropriate `IHandleAsync<…>` handler via `[FromServices]`
2. **Handlers** process the business logic and return `Result` / `Result<T>` objects
3. **Extension methods** from `MiddleMan.Zero.AspNetCore.Mvc` / `MiddleMan.Zero.AspNetCore.Http` convert results to HTTP responses:

| ResultStatus | HTTP Status Code |
|---|---|
| `Successful` | 200 OK |
| `NotFound` | 404 Not Found |
| `Invalid` | 400 Bad Request |
| `Forbidden` | 403 Forbidden |
| `Failure` | 500 Internal Server Error |

## Project Structure

```
IceCreamTruck.WebApi/
├── Controllers/
│   └── OrdersController.cs          # MVC controller: POST/GET/DELETE orders
├── Endpoints/
│   └── FlavorEndpoints.cs           # Minimal API endpoints: GET/POST flavors
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── IceCreamTruck.WebApi.csproj
```

## Dependencies

- **IceCreamTruck**: Core business logic library
- **MiddleMan.Zero.AspNetCore.Mvc**: MVC Controller integration
- **MiddleMan.Zero.AspNetCore.Http**: Minimal API integration
- **Swashbuckle.AspNetCore**: OpenAPI/Swagger documentation
