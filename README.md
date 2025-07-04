# Communication Service

## Overview

This project is a .NET 8 REST API for sending personalized messages to customers using templates. It uses JWT for authentication, Entity Framework Core with SQLite for persistence, and exposes its documentation via Swagger.

> **Note:** Before you can log in and access protected endpoints, you must first create a user account using the `POST /api/v1/users` endpoint. Once your user is created, you can log in and use your JWT to access the other endpoints.

## Domain Models and Validation

### Customer
- **Name**: String, 2 to 64 characters (required)
- **Email**: Valid email format, up to 320 characters (required)

### Template
- **Name**: String, 2 to 64 characters (required)
- **Subject**: String, 3 to 128 characters (required)
- **Body**: String, 0 to 1024 characters (required)

### User
- **Email**: Valid email format, up to 320 characters (required)
- **Password**: String, 6 to 32 characters (required)

All fields are validated server-side according to these constraints. Any attempt to create or update a model that does not meet these requirements will result in a validation error.

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
sudo dotnet run
```

The API will be available at `https://localhost:7079` or `http://localhost:5052` by default (see `launchSettings.json`).

## Configuration

You can configure the JWT and SMTP settings in `DanskeBank.Communication/appsettings.json`.

## API Documentation (Swagger)

Once the API is running, go to [http://localhost:5052/swagger](http://localhost:5052/swagger) to explore and test all endpoints via Swagger UI.

## Main Endpoints

### Authentication
- `POST /api/v1/auth/login`: Login, returns a JWT
- `POST /api/v1/auth/refresh`: Refresh the JWT

### Users
- `GET /api/v1/users`: List users (paginated)
- `GET /api/v1/users/{id}`: Get user by ID
- `POST /api/v1/users`: Create user (anonymous access allowed)
- `PUT /api/v1/users/{id}`: Update user
- `DELETE /api/v1/users/{id}`: Delete user

### Customers
- `GET /api/v1/customers`: List customers (paginated)
- `GET /api/v1/customers/{id}`: Get customer by ID
- `POST /api/v1/customers`: Create customer
- `PUT /api/v1/customers/{id}`: Update customer
- `DELETE /api/v1/customers/{id}`: Delete customer
- `GET /api/v1/customers/search?query=...`: Search customers by name or email

### Templates
- `GET /api/v1/templates`: List templates (paginated)
- `GET /api/v1/templates/{id}`: Get template by ID
- `POST /api/v1/templates`: Create template
- `PUT /api/v1/templates/{id}`: Update template
- `DELETE /api/v1/templates/{id}`: Delete template
- `GET /api/v1/templates/search?query=...`: Search templates by name or content
- `POST /api/v1/templates/{templateId}/send/{customerId}`: Send a template to a customer (performs variable replacement)

## Authentication

Most endpoints require a JWT in the header, formatted as `Authorization: Bearer <token>`. However, the user creation endpoint (`POST /api/v1/users`) is open for testing purposes. Authentication endpoints (`/api/v1/auth/*`) are always accessible anonymously.

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

## Email Body Formatting

The `Body` field of email templates must be written in **HTML**. This allows you to send rich, well-formatted emails to your customers.

- Use standard HTML tags (`<p>`, `<b>`, `<a>`, etc.) to structure your content.
- Avoid JavaScript or complex CSS styles (many email clients do not support them).
- You can insert dynamic variables into the HTML body, which will be replaced when sending.

**Available dynamic variables:**
- `{{Customer.Name}}` — The customer's name
- `{{Customer.Email}}` — The customer's email address
- `{{Customer.CensoredEmail}}` — The customer's email address, partially censored for privacy
- `{{Sender.Email}}` — The sender's (SMTP user) email address
- `{{Date}}` — The current date in `yyyy-MM-dd` format (UTC)

**Example HTML body:**

```html
<p>Hello <b>{{Customer.Name}}</b>,</p>
<p>Your registered email is: {{Customer.Email}}</p>
<p>Masked email: {{Customer.CensoredEmail}}</p>
<p>This message was sent by: {{Sender.Email}} on {{Date}}</p>
```

The HTML content will be sent as-is in the email body. Always check the rendering in an email client before sending to your users.

---

For any questions, refer to the source code or the automatically generated Swagger documentation.