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

1. Customer creates an order with `CreateOrderMessage`
2. `CreateOrderHandler` validates and processes the order
3. Order can be retrieved later with `GetOrderMessage` and `GetOrderHandler`

This simple domain allows us to focus on demonstrating MiddleMan.Zero features rather than complex business logic.
