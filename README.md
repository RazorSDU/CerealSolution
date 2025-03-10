# Cereal API

## Overview
This is a REST API for managing and querying nutritional data about various breakfast cereals.  
The API supports:

- **CRUD operations** (Create, Read, Update, Delete) on a `Cereal` entity.
- **Filtering** by nutritional values (calories, protein, etc.).
- **Authentication & Authorization** for secured endpoints.
- **Image retrieval** for cereals (with a fallback placeholder).

## Table of Contents
1. [Requirements](#requirements)
2. [Setup & Installation](#setup--installation)
3. [Usage](#usage)
4. [API Endpoints](#api-endpoints)
5. [Authentication](#authentication)
6. [Design Choices](#design-choices)
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
1. **ASP.NET Core Web API**  
   - Chosen because of personal experience, and the stability of it.
2. **Entity Framework Core**  
   - Simplifies database operations and migrations.  
   - Using **InMemory** mode for unit tests, and **SQL Server** or **LocalDB** for production.
3. **CSV Importer**  
   - Automatically seeds cereals from `Cereal.csv` on app startup.  
   - Allows adding or updating missing images or cereals.
4. **Image Handling**  
   - Stores **image paths** in the DB, actual files in `Data/Images`.  
   - Returns a placeholder if no cereal-specific image is found.
5. **Unit Tests**  
   - **xUnit** test project to verify **CRUD operations**, **filtering**, and **authorization** logic.  
   - Uses **Moq** for mocking and **EF Core InMemory** for testing DB interactions.

---

## Contributing
1. Please don't?
