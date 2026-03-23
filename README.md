# Foundation

[![Build Status](https://github.com/rocksolidorg/foundation-dotnet/actions/workflows/publish.yml/badge.svg)](https://github.com/rocksolidorg/foundation-dotnet/actions)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)

A lightweight .NET library that provides foundational building blocks for Tactical Domain-Driven Design (DDD).

## Features

- **Aggregate Roots, Entities, and Value Objects**: Strongly-typed modeling primitives for DDD.
- **Domain Events**: Built-in support for domain event dispatching and handling.
- **Unit of Work**: Transactional consistency and repository abstraction.
- **EF Core Integration**: Helpers for Entity Framework Core, including conventions and repository patterns.
- **ASP.NET Core Extensions**: Easy DI registration for DDD services.
- **Test Coverage**: Comprehensive unit and integration tests.

## Getting Started

### Prerequisites

- [.NET 8.0, 9.0, or 10.0](https://dotnet.microsoft.com/)
- (Optional) [EF Core](https://docs.microsoft.com/en-us/ef/core/) for persistence

### Installation

Add the NuGet package (coming soon):

```sh
dotnet add package RockSolid.Foundation.Modeling.AspNetCore --prerelease
```

### Usage Example

```csharp
builder.Services.AddDomainEventDispatcher();
builder.Services.AddDomainEventHandler<MyHandler>();
builder.Services.AddUnitOfWork<ApplicationDbContext>();
```

See the Demo for a working example.

## Building and Testing

- [Task](https://github.com/go-task/task)

To build and test the project:

```sh
task test
```

## Project Structure

- src — Core libraries and ASP.NET Core integration
- tests — Unit and integration tests

## Contributing

Not accepting contributions at the moment.

## License

Copyright 2026 John Susi

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0


