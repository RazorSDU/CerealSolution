# Cereal API

## Overview
This is a REST API for managing and querying nutritional data about various breakfast cereals.  
The API supports:

- **CRUD operations** (Create, Read, Update, Delete) on a `Cereal` entity.
- **Filtering** by nutritional values (calories, protein, etc.).
- **Sorting** also by nutritional values (calories, protein, etc.).
- **Authentication & Authorization** for secured endpoints.
- **Image retrieval** for cereals (with a fallback placeholder).
- **Rate Limiting** to prevent excessive requests.
- **Serilog logging** for detailed runtime diagnostics.

## Table of Contents
1. [Requirements](#requirements)
2. [Setup & Installation](#setup--installation)
3. [Usage](#usage)
4. [API Endpoints](#api-endpoints)
5. [Authentication](#authentication)
6. [Design Choices](#design-choices)
    - [EF Core Connections](#ef-core-connections)
    - [Why Not a Singleton DbContext](#why-not-a-singleton-dbcontext)
    - [Why a Factory Pattern Is Not Needed](#why-a-factory-pattern-is-not-needed)
    - [Rate Limiting & HTTPS Enforcement](#rate-limiting--https-enforcement)
7. [Contributing](#contributing)

---

## Requirements
- **.NET 8**
- **SQL Server** or **LocalDB** (for the database)
- **Visual Studio 2022** (recommended for development)

---

## Setup & Installation
1. **Clone** this repository:
   ```bash
   git clone https://github.com/RazorSDU/CerealSolution.git
   ```
2. **Open** `CerealApi.sln` in **Visual Studio 2022**.
3. **Restore NuGet packages** (Visual Studio will do this automatically).
4. **Ensure** your `appsettings.json` has a valid connection string under `"CerealDatabase"`.
5. Run the **migrations** or let EF Core create the DB on first run:
   ```powershell
   Update-Database
   ```
6. **Place** `Cereal.csv` and any **Images** in the `Data` folder.

---

## Usage
1. **Build** & **Run** the solution (`F5` in Visual Studio).
2. The API will be available at:
   ```
   https://localhost:<PORT>/api/cereal
   ```
3. Use **Swagger** (auto-generated docs) at:
   ```
   https://localhost:<PORT>/swagger
   ```
4. **CSV Import** runs automatically at startup, parsing `Cereal.csv` into the DB if not already present.
5. **HTTPS is enforced** in production to ensure secure data transmission.
6. **Rate limiting** is enabled (in production and development) to prevent excessive requests.

---

## API Endpoints

### **Public (No Auth Required)**
- `GET /api/cereal`  
  - Retrieves all cereals or filters them using query parameters like `CaloriesMin`, `CaloriesMax`, `Name`, etc.
  - Example: `/api/cereal?Name=Bran&CaloriesMin=70`
- `GET /api/cereal/{id}`  
  - Retrieves a single cereal by its ID.
- `GET /api/cereal/{id}/image`  
  - Returns the cereal’s image or a placeholder if the cereal has no image.

### **Protected (Auth Required)**
- `POST /api/cereal`  
  - **If `Id == 0`** → Creates a new cereal.  
  - **If `Id != 0` and exists** → Updates existing cereal.  
  - **If `Id != 0` but not found** → Returns an error.
- `PUT /api/cereal/{id}`  
  - Updates an existing cereal by ID.
- `DELETE /api/cereal/{id}`  
  - Deletes an existing cereal by ID.

---

## Authentication
1. **User Registration** & **Login** endpoints (AuthController)  
   - `POST /api/auth/register` → Creates a new user with **BCrypt-hashed** password.  
   - `POST /api/auth/login` → Returns a **JWT** if credentials are valid.
2. **Using JWT**  
   - Include the token in **Authorization** header:  
     ```
     Authorization: Bearer <your-jwt-token>
     ```
   - This grants access to **POST, PUT, DELETE** methods.

---

## Design Choices

### EF Core Connections
**Why EF Opens and Closes Connections Often**  
When EF Core executes a query or command, it briefly:
- Opens a physical connection if none is open.
- Executes the SQL.
- Closes/disposes that logical connection when done.

However, behind the scenes, **ADO.NET connection pooling** is usually caching these physical connections. So while EF logs that it’s “opening” or “closing” a connection, it’s not always a brand-new TCP connection to SQL Server. Instead, it’s often just grabbing a pooled connection and then returning it to the pool. This is normal and desired behavior.

If you see a lot of open/close messages in your logs, that is often because you are logging at a **verbose** or **trace** level (like we do with Serilog). It can look noisy, but it’s typically not harmful to performance.

### Why Not a Singleton DbContext
1. **Thread Safety & Concurrency**  
   EF Core’s **DbContext** is not thread-safe. A single context instance handling multiple requests simultaneously can cause concurrency issues, stale data, or worse.
2. **Unit of Work / Lifetime**  
   The recommended pattern in web apps is **per-request scoping**:
   - Each HTTP request gets its own `DbContext`.
   - At the end of the request, that `DbContext` is disposed.  
   This ensures each request does its own “unit of work” in isolation.
3. **Change Tracking Overload**  
   If a DbContext lives too long, any entity you retrieve might remain in its change-tracking cache indefinitely, leading to memory bloat and unexpected behavior.
4. **Connection Pooling**  
   ADO.NET automatically handles pooling. You don’t need a single, never-closed connection to avoid overhead. The typical overhead of opening/closing in EF is just returning the connection to the pool.

### Why a Factory Pattern Is Not Needed
A factory pattern for creating multiple `DbContext` instances or repositories is useful in **complex** scenarios with multiple entities or varied database access patterns.  
In this project, we are primarily dealing with **a single entity/table (`Cereal`)**. The built-in **Dependency Injection** with EF Core already provides:
- Scoped `DbContext` creation per request
- Automated disposal
- Straightforward usage in controllers and services

Hence, **no specialized factory** is necessary.

### Rate Limiting & HTTPS Enforcement
- **Rate Limiting**  
  We use `AddRateLimiter` with a global IP-based rate limiter to prevent abuse. A short, fixed window helps demonstrate and test this feature easily. In production, the window and limit can be adjusted as needed.
- **HTTPS Enforcement**  
  - The app uses **`UseHttpsRedirection()`** in non-test environments to encourage secure connections.  
  - **HSTS** is enabled in production.  
  - Any plain `HTTP` request in production is blocked with a **`403 Forbidden`** response, ensuring data is transmitted securely.

---

## Contributing
1. Please don’t?  
   - Currently, this is a personal/educational project.  
   - If you want to fork and adapt it, go for it!
