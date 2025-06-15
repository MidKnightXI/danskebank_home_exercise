# Communication Service

## Overview

This project is a .NET 8 REST API for sending personalized messages to customers using templates. It uses JWT for authentication, Entity Framework Core with SQLite for persistence, and exposes its documentation via Swagger.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- (Optional) [Docker](https://www.docker.com/)

## Setup & Run

1. **Clone the repository**

```bash
git clone git@github.com:MidKnightXI/danskebank_home_exercise.git
cd danskebank_home_exercise
```

2. **Restore dependencies**

```bash
dotnet restore
```

3. **Run the database and API**

```bash
cd DanskeBank.Communication
dotnet run
```

The API will be available at `https://localhost:7079` or `http://localhost:5052` by default (see `launchSettings.json`).

## Configuration

You can configure the JWT settings in `DanskeBank.Communication/appsettings.json`.

## API Documentation (Swagger)

Once the API is running, go to [https://localhost:7079/swagger](https://localhost:7079/swagger) to explore and test all endpoints via Swagger UI.

## Main Endpoints

- **Authentication**
  - `POST /api/v1/auth/login`: Login, returns a JWT
  - `POST /api/v1/auth/refresh`: Refresh the JWT
- **Users**
  - `GET /api/v1/users`: List users
  - `POST /api/v1/users`: Create user
  - `PUT /api/v1/users/{id}`: Update user
  - `DELETE /api/v1/users/{id}`: Delete user
- **Customers**
  - `GET /api/v1/customers`: List customers
  - `POST /api/v1/customers`: Create customer
  - `PUT /api/v1/customers/{id}`: Update customer
  - `DELETE /api/v1/customers/{id}`: Delete customer
  - `GET /api/v1/customers/search?query=...`: Search customers
- **Templates**
  - `GET /api/v1/templates`: List templates
  - `POST /api/v1/templates`: Create template
  - `PUT /api/v1/templates/{id}`: Update template
  - `DELETE /api/v1/templates/{id}`: Delete template
  - `GET /api/v1/templates/search?query=...`: Search templates
  - `POST /api/v1/templates/{templateId}/send/{customerId}`: Send a template to a customer

## Authentication

Most endpoints require a JWT in the header, formatted as `Authorization: Bearer <token>`. However, there are exceptions: the user creation endpoint is exempt from this requirement for testing purposes, but it would require admin authentication in a production environment. Additionally, the authentication-related endpoints always permit anonymous access.

## Main Dependencies
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore.Sqlite
- Swashbuckle.AspNetCore (Swagger)
- prometheus-net.AspNetCore (metrics)

## Test Dependencies
- Microsoft.NET.Test.Sdk
- xunit
- xunit.runner.visualstudio
- coverlet.collector
- Microsoft.EntityFrameworkCore.InMemory
- System.Net.Http
- System.Text.RegularExpressions

## Tests

To run unit tests:

```bash
cd DanskeBank.Communication.Tests
dotnet test
```

## Docker

You can also run the API using Docker:

1. **Build the Docker image**

```bash
docker build -t danskebank-communication ./DanskeBank.Communication
```

2. **Run the container**

```bash
docker run -p 8080:80 danskebank-communication
```

The API will be available at `http://localhost:8080` (inside the container it listens on port 80).

## Prometheus Metrics

This application exposes a Prometheus-compatible metrics endpoint at `/metrics`.

---

For any questions, refer to the source code or the automatically generated Swagger documentation.