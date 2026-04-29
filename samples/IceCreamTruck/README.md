# IceCreamTruck - MiddleMan.Zero Sample Domain

This is the central domain package for all MiddleMan.Zero examples. It simulates an ice cream truck business with orders, products, and handlers.

## Domain Overview

The IceCreamTruck domain includes:

- **Models**: Core business entities (IceCream, Order, OrderStatus)
- **Messages**: Request/response definitions using MiddleMan.Zero abstractions
- **Handlers**: Business logic for processing messages

## Usage

This package is referenced by various sample applications that demonstrate different aspects of MiddleMan.Zero:

- Console applications
- Web APIs
- Dependency injection scenarios
- Advanced patterns (validation, pipelines, etc.)

## Example Flow

1. Customer creates an order with `CreateOrderRequest` → `CreateOrderHandler` validates and processes it
2. Order can be retrieved later with `GetOrderRequest` → `GetOrderHandler`
3. Admins can cancel an order with `CancelOrderRequest` → `CancelOrderHandler`
   - Non-admin callers receive a **Forbidden** result
   - Cancelling a non-existent order returns a **NotFound** result

This simple domain allows us to focus on demonstrating MiddleMan.Zero features rather than complex business logic.

## Handlers

| Handler | Request | Response | Statuses demonstrated |
|---------|---------|----------|-----------------------|
| `CreateOrderHandler` | `CreateOrderRequest` | `Guid` | Successful, Invalid |
| `GetOrderHandler` | `GetOrderRequest` | `Order` | Successful, Invalid, NotFound |
| `AddFlavorHandler` | `AddFlavorRequest` | _(void)_ | Successful, Invalid |
| `GetFlavorsHandler` | `GetFlavorsRequest` | `Flavor[]` | Successful |
| `CancelOrderHandler` | `CancelOrderRequest` | _(void)_ | Successful, Invalid, NotFound, **Forbidden** |
