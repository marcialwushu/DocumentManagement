# Document Management System

A modern document management system built with .NET using Clean Architecture principles and gRPC for efficient communication.

## Project Structure

The solution is organized into four main projects following Clean Architecture principles:

- **DocumentManagement.Domain**: Contains enterprise business rules and entities
- **DocumentManagement.Application**: Contains business rules and interfaces
- **DocumentManagement.Infrastructure**: Contains external concerns implementations
- **DocumentManagement.GrpcServer**: Contains the gRPC service implementations

## Architecture

This project follows Clean Architecture principles to maintain:

- Independence of frameworks
- Testability
- Independence of UI
- Independence of Database
- Independence of any external agency

### Key Features

- Clean Architecture implementation
- gRPC communication for efficient data transfer
- Separation of concerns
- Domain-driven design principles
- Infrastructure abstraction

## Getting Started

### Prerequisites

- .NET 7.0 SDK or later
- Visual Studio 2022 or compatible IDE

### Installation

1. Clone the repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the solution

```bash
dotnet restore
dotnet build
```

### Running the Application

1. Set DocumentManagement.GrpcServer as the startup project
2. Run the application

```bash
dotnet run --project src/DocumentManagement.GrpcServer
```

## Development

### Adding New Features

1. Implement domain entities in DocumentManagement.Domain
2. Create use cases in DocumentManagement.Application
3. Implement infrastructure concerns in DocumentManagement.Infrastructure
4. Expose functionality through gRPC services in DocumentManagement.GrpcServer

## License

This project is licensed under the MIT License - see the LICENSE file for details