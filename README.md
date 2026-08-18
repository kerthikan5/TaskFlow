# TaskFlow — Task & Project Management System

<div align="center">

![TaskFlow Banner](docs/banner.png)

[![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)](#)
[![Tests](https://img.shields.io/badge/tests-passing-brightgreen?style=flat-square)](#)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](#)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react)](#)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](#)

**A full-stack, production-grade task and project management platform built from scratch with Clean Architecture.**

[Features](#-features) · [Architecture](#-architecture) · [Getting Started](#-getting-started) · [API Reference](#-api-reference) · [Testing](#-testing)

</div>

---

## 📋 Overview

TaskFlow is a portfolio-level project demonstrating **Clean Architecture**, **JWT Authentication**, **role-based access control**, **EF Core with SQLite**, and a **React 19 TypeScript frontend** — built to production standards without shortcuts.

It was built as a comprehensive full-stack engineering exercise covering backend API design, database modeling, security, unit & integration testing, and modern frontend development.

---

## ✨ Features

### 🔐 Authentication & Security
- **JWT Bearer Token** authentication with configurable expiry
- Secure **password hashing** using `PasswordHasher<T>` (PBKDF2)
- Global **ExceptionHandlingMiddleware** with RFC-7807 `ProblemDetails` responses
- Strict **DTO separation** to prevent data leakage

### 📁 Project Management
- Create and manage multiple projects
- Project **owner-only** update/delete authorization
- Project **status tracking** (Active, On Hold, Completed, Archived)

### 👥 Team Collaboration
- Invite team members by **email address**
- Project-level membership access control
- Members can **leave** projects; owners can **remove** members
- Owner cannot be removed from their own project

### ✅ Task Management
- Create tasks within projects with **priority levels** (Low → Critical)
- Assign tasks to project members with **membership validation**
- **Kanban-style** workflow: `ToDo → InProgress → Completed → Cancelled`
- Quick status update via `PATCH /api/tasks/{id}/status`
- **My Assigned Tasks** — view all tasks assigned to you across all projects

### 🔍 Filtering & Pagination
- Filter tasks by **Status**, **Priority**, **Assignee**
- **Full-text search** on task title and description
- `PagedResponse<T>` with total count and page metadata

### 🖥️ Frontend
- **Kanban board** view per project
- **Dashboard** with stats, recent projects, and assigned tasks
- **My Tasks** table with inline status updates
- Responsive dark-mode interface

---

## 🏗️ Architecture

This project follows **Clean Architecture** (also known as Onion Architecture), enforcing strict dependency rules:

```
┌─────────────────────────────────────────────┐
│                  API Layer                   │  ← Controllers, Middleware, DI
├─────────────────────────────────────────────┤
│             Infrastructure Layer             │  ← EF Core, JWT, Persistence
├─────────────────────────────────────────────┤
│              Application Layer               │  ← Services, Interfaces, DTOs, Exceptions
├─────────────────────────────────────────────┤
│                Domain Layer                  │  ← Entities, Enums, Business Rules
└─────────────────────────────────────────────┘
```

**Dependency Rule**: Each layer only depends on layers *below* it. The Domain layer has **zero external dependencies**.

### Project Structure

```
TaskFlow/
├── src/
│   ├── TaskFlow.Domain/              # Entities, Enums
│   ├── TaskFlow.Application/         # Service interfaces, DTOs, Exceptions
│   ├── TaskFlow.Infrastructure/      # EF Core, JWT, CurrentUserService
│   └── TaskFlow.Api/                 # ASP.NET Core Controllers, Middleware
│
├── tests/
│   ├── TaskFlow.UnitTests/           # xUnit unit tests (JWT, services)
│   └── TaskFlow.IntegrationTests/    # WebApplicationFactory end-to-end tests
│
└── client/
    └── taskflow-web/                 # React 19 + TypeScript + Vite frontend
```

---

## 🛠️ Technology Stack

| Layer | Technology |
|-------|-----------|
| **Runtime** | .NET 10 (ASP.NET Core) |
| **ORM** | Entity Framework Core 10 |
| **Database** | SQLite (zero-config local dev) |
| **Authentication** | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) |
| **Password Hashing** | ASP.NET Core Identity PasswordHasher |
| **API Docs** | .NET 10 Native OpenAPI (`/openapi/v1.json`) |
| **Frontend** | React 19, TypeScript, Vite 8 |
| **HTTP Client** | Axios (with JWT interceptors) |
| **Frontend Routing** | React Router v6 |
| **Icons** | Lucide React |
| **Unit Testing** | xUnit, Moq |
| **Integration Testing** | Microsoft.AspNetCore.Mvc.Testing |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/TaskFlow.git
cd TaskFlow
```

### 2. Configure Backend

The app uses **SQLite** — no database setup required. The file `taskflow.db` is created automatically.

Review JWT settings in `src/TaskFlow.Api/appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-chars",
    "Issuer": "TaskFlowApi",
    "Audience": "TaskFlowClient",
    "ExpiryMinutes": 60
  }
}
```

> ⚠️ For production, use environment variables or .NET User Secrets for `Jwt:Secret`.

### 3. Run Database Migrations

```bash
cd src/TaskFlow.Api
dotnet ef database update --project ../TaskFlow.Infrastructure
```

### 4. Start the Backend API

```bash
cd src/TaskFlow.Api
dotnet run
```

API will be available at: `http://localhost:5108`
OpenAPI spec: `http://localhost:5108/openapi/v1.json`

### 5. Start the Frontend

```bash
cd client/taskflow-web
npm install
npm run dev
```

Frontend will be available at: `http://localhost:5173`

---

## 📡 API Reference

### Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/auth/register` | ❌ | Register a new user |
| `POST` | `/api/auth/login` | ❌ | Login and get JWT token |
| `GET` | `/api/auth/me` | ✅ | Get current user profile |

### Users

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `PUT` | `/api/users/me` | ✅ | Update own profile |

### Projects

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/projects` | ✅ | Get accessible projects |
| `POST` | `/api/projects` | ✅ | Create project |
| `GET` | `/api/projects/{id}` | ✅ Owner/Member | Get project details |
| `PUT` | `/api/projects/{id}` | ✅ Owner only | Update project |
| `DELETE` | `/api/projects/{id}` | ✅ Owner only | Delete project |

### Project Members

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/projects/{id}/members` | ✅ Owner/Member | List members |
| `POST` | `/api/projects/{id}/members` | ✅ Owner only | Invite by email |
| `DELETE` | `/api/projects/{id}/members/{userId}` | ✅ Owner or Self | Remove/Leave |

### Tasks

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/projects/{id}/tasks` | ✅ Owner/Member | Get project tasks (paginated) |
| `POST` | `/api/projects/{id}/tasks` | ✅ Owner/Member | Create task |
| `GET` | `/api/tasks/my-assigned` | ✅ | My assigned tasks |
| `GET` | `/api/tasks/{id}` | ✅ Owner/Member | Get task by ID |
| `PUT` | `/api/tasks/{id}` | ✅ Owner/Member | Update task |
| `PATCH` | `/api/tasks/{id}/status` | ✅ Owner/Member | Update task status |
| `DELETE` | `/api/tasks/{id}` | ✅ Owner/Member | Delete task |

### Query Parameters (Tasks)

```
GET /api/projects/{id}/tasks?status=0&priority=2&assigneeId=xxx&searchTerm=bug&pageNumber=1&pageSize=10
```

---

## 🧪 Testing

### Run All Tests

```bash
dotnet test TaskFlow.slnx
```

### Unit Tests

Located in `tests/TaskFlow.UnitTests/`:
- `JwtTokenGeneratorTests` — verifies JWT token claims and signature

### Integration Tests

Located in `tests/TaskFlow.IntegrationTests/`:
- `AuthIntegrationTests` — register, login, bearer token access (`/api/auth/me`)
- `ProjectTaskIntegrationTests` — full end-to-end workflow:
  1. Register Owner + Member
  2. Owner creates Project
  3. Owner invites Member by email
  4. Owner creates Task and assigns to Member
  5. Member updates Task status to `Completed`

Integration tests use `WebApplicationFactory` with an **isolated, in-memory SQLite database** — no shared state between runs.

---

## 🔒 Security Design Decisions

| Concern | Implementation |
|---------|---------------|
| Password storage | PBKDF2 via `PasswordHasher<User>` (no plain-text, no MD5/SHA1) |
| Auth tokens | Short-lived JWT with configurable expiry |
| Input validation | Custom `ValidationException` → `400 Bad Request` |
| Not found | `NotFoundException` → `404 Not Found` |
| Access control | `ForbiddenException` → `403 Forbidden` |
| Conflicts | `ConflictException` → `409 Conflict` |
| Error format | RFC-7807 `ProblemDetails` via middleware |
| DTO separation | Entities never exposed directly — mapped to DTOs |
| .gitignore | `taskflow.db`, `*.db-journal`, `appsettings.*.json`, `.env` |

---

## 📁 Domain Model

```
User ─────────────────────────────────────────────┐
 │ owns                                            │ is member of
 ▼                                                 ▼
Project ──── has many ──── ProjectMember (join)
 │
 └── has many ──── TaskItem
                     │ assigned to ──── User
                     │ created by  ──── User
```

---

## 🛡️ Authorization Matrix

| Action | Anonymous | Member | Owner |
|--------|-----------|--------|-------|
| Register / Login | ✅ | ✅ | ✅ |
| View Projects | ❌ | ✅ (own) | ✅ |
| Create Project | ❌ | ✅ | ✅ |
| Edit / Delete Project | ❌ | ❌ | ✅ |
| View Members | ❌ | ✅ | ✅ |
| Add Member | ❌ | ❌ | ✅ |
| Remove Member | ❌ | Self only | ✅ |
| Create / View Tasks | ❌ | ✅ | ✅ |
| Update Task Status | ❌ | ✅ | ✅ |

---

## 🤝 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for branch, commit, and PR conventions.

---

## 📄 License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.

---

<div align="center">
Built with ❤️ as a portfolio project demonstrating production-grade software engineering.
</div>
