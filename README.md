# HabitTrack API
Habit tracking REST API built with ASP.NET Core

## 📌 Description
HabitTrack is a RESTful API that allows users to manage daily habits, track their completion, 
organize them by categories, and visualize progress over time.

This project is built as a personal learning project focused on clean architecture,
domain-driven design principles, and backend best practices using .NET.

## 🧠 Main Features
  - User registration and authentication (JWT)
  - Create, update, delete, and retrieve habits
  - Habit categorization
  - Daily habit tracking using logs
  - Soft delete support
  - User-scoped data

## 🏗️ Architecture
The project follows a layered architecture:
  - API layer
  - Application layer (Use cases, services, DTOs)
  - Domain layer (Entities, enums, business rules)
  - Infrastructure layer (EF Core, repositories, database)

## ⚙️ Setup & Run
1. Clone the repository
2. the connection string can be configured in `appsettings.json` or environment variables depending on the environment. 
4. Run database migrations:
   dotnet ef database update
5. Run the application:
   dotnet run

## 🔐 Authentication
This API uses JWT Bearer authentication.  
Protected endpoints require a valid access token in the `Authorization` header.

## 📚 Notes
This project is intended as a learning exercise and portfolio project.
Feedback and suggestions are welcome. ;)
