# GoAir - ASP.NET Advanced Project Assignment

GoAir is a simple airline management web application built with the idea to simulate how an airline might manage its flights, aircraft, and airports.

The idea is to simplify the complexity of a real-world airline system and try to reduce it to simple core features that can be scaled up in the future. Users can browse flights, airports, and aircraft, while authenticated users and administrators have access to more advanced functionality.

The project was extended into a more complete ASP.NET Core application with layered architecture, Identity, role-based access, service logic, validation, seeding, and tests.

## Main features

- Browse flights with search, sorting, and pagination
- View flight, airport, and aircraft details
- Register and log in with ASP.NET Core Identity
- Create, edit, and delete tickets/reviews when authenticated
- Have a dedicated `Administration` area for managing the catalog
- Work with seeded demo data for airports, aircraft, flights, roles, and an admin account

## Roles

- `Guest`
  Can browse the public catalog such as the home page, flights, airports, and aircraft.
- `User`
  Can manage only their own tickets and reviews.
- `Administrator`
  Can access the `Administration` area and manage flights, airports, aircraft, tickets, and reviews.

> Newly registered users are assigned the `User` role automatically. For testing purposes, a seeded administrator account is also created for local testing and demo purposes - check out `appsettings.json` for the login.

## Project structure

The solution is separated into multiple projects:

- `GoAir.Data` - EF Core `DbContext`, migrations, and data access configuration.
- `GoAir.Data.Common` - Constants for all entities.
- `GoAir.Data.Models` - Domain entities such as `Flight`, `Airport`, `Aircraft`, `Ticket`, `Review`, and `ApplicationUser`.
- `GoAir.Services.Common` - A unified service operation result that tracks success, not-found, and keyed validation errors without throwing exceptions.
- `GoAir.Services.Core` - Business logic, validations, ownership checks, filtering, paging, and service orchestration.
- `GoAir.IntegrationTests` - Basic integration test coverage around the data layer.
- `GoAir.Services.Core.Tests` - Unit tests for the service layer.
- `GoAir.Web.Tests` - Tests for authorization conventions in the web layer.
- `GoAir.Web` - ASP.NET Core MVC application with controllers, Razor views, Identity pages, and the administration area.
- `GoAir.Web.ViewModels` - View models used by the MVC layer.
- `GoAir.GCommon` - Shared constants such as application roles.

## Business logic and validation

Some of the main business rules implemented in the services are:

- Flights cannot use the same aircraft in overlapping time ranges
- Flights must have valid and different departure and arrival airports
- Airports must use unique IATA codes
- Aircraft cannot be deleted while assigned to flights
- Airports cannot be deleted while used by flights
- Flights cannot be deleted while they still have tickets or reviews
- Ticket seats must be unique per flight
- Tickets and reviews are tied to the currently signed-in user
- Future purchase and review dates are rejected

Validation is handled both on the client side and on the server side. Razor validation helpers and unobtrusive validation are used in the UI, while the service layer performs the main server-side checks before data is saved.

## Technologies

- Frontend - Razor Views, Bootstrap, jQuery, JavaScript
- Backend - ASP.NET Core MVC (.NET 8, C# 12)
- Database - Entity Framework Core with SQL Server
- Authentication - ASP.NET Core Identity
- Testing - NUnit

## Testing

- `GoAir.IntegrationTests` - 1 passing test
- `GoAir.Services.Core.Tests` - 55 passing tests
- `GoAir.Web.Tests` - 23 passing tests
- In total - 79 passing tests, `87.41%` line coverage

### Commands for running the tests

```
dotnet test src/GoAir.Services.Core.Tests/GoAir.Services.Core.Tests.csproj
dotnet test src/GoAir.Web.Tests/GoAir.Web.Tests.csproj
dotnet test src/GoAir.IntegrationTests/GoAir.IntegrationTests.csproj
```

## Getting started

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB
- Git

### 1. Clone the repository

```
git clone https://github.com/velislav088/GoAir.git
cd GoAir
```

### 2. Restore and build
```
dotnet restore
dotnet build
```

### 3. Build the database
```
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/GoAir.Data --startup-project src/GoAir.Web
```
> Installing the Entity Framework CLI may not be neccessary, but it fixes any errors if the database doesn't migrate properly.

> If the process is done through Visual Studio, select `src/Data/GoAir.Data` as the default project from the Package Manager Console and `GoAir.Web` as the startup project.

### 4. Run the app
```
dotnet run --project src/GoAir.Web
```

### 5. Open a browser and navigate to the localhost port given by the console output

### 6. Try out the features
> The models are connected logically, there can't be a flight without an existing aircraft or an airport and tickets/reviews need a given flight to be submitted.
