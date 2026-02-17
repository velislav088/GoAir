# GoAir - ASP.NET Fundamentals Project Assignment

GoAir is a simple airline management web application built with the idea to simulate how an airline might manage its flights, aircraft, and airports.
The idea is to simplify the complexity of a real-world airline system and try to reduce it to simple core features that can be scaled up in the future.

## Main features

- Browse, create, edit and delete flights
- Manage aircraft and airports
- Track flight schedules and availability

## Technologies

- Frontend - Razor Views, Bootstrap, jQuery, JavaScript
- Backend - ASP.NET Core MVC (.NET 10, C# 14)
- Database - Entity Framework Core with SQL Server

## Getting started

### Prerequisites

- .NET 10 SDK
- Git (to clone the repo)

### 1. Clone the repository

```bash
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
dotnet ef database update
```

### 4. Run the app
```
dotnet run
```

### 5. Open a browser and navigate to `https://localhost:7294`

### 6. Try out the features
> Flights need an Aircraft and an Airport to be created first, so start by creating some airports and aircrafts before creating flights.