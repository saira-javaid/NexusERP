# NexusERP — Enterprise Project Management System

Full-stack enterprise PMS built with **ASP.NET Core 9** and **Angular 20**.

## Architecture

```
NexusERP/
├── backend/                    # ASP.NET Core Clean Architecture
│   ├── src/
│   │   ├── NexusERP.Domain/           # Entities, enums, domain interfaces
│   │   ├── NexusERP.Application/      # CQRS (MediatR), DTOs, validators
│   │   ├── NexusERP.Infrastructure/   # EF Core, Identity, Redis, SignalR
│   │   └── NexusERP.API/              # Web API, middleware, DI
│   └── tests/
│       └── NexusERP.Tests/            # xUnit tests
└── frontend/
    └── nexus-erp-web/          # Angular 20 SPA
```

### Backend Layers

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Entities, value objects, repository interfaces, domain events |
| **Application** | Commands/queries (CQRS), DTOs, AutoMapper profiles, FluentValidation |
| **Infrastructure** | EF Core, ASP.NET Identity, JWT, Redis, SignalR hubs, background services |
| **API** | Controllers, middleware, Swagger, health checks |

### Frontend Structure

| Folder | Responsibility |
|--------|----------------|
| **core** | Auth, interceptors, guards, services, layout shell |
| **shared** | Reusable components, directives, pipes, models |
| **features** | Lazy-loaded feature modules (projects, tasks, kanban, etc.) |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [Angular CLI 20](https://angular.dev/) — `npm install -g @angular/cli@20`
- [Docker Desktop](https://www.docker.com/) (optional, for SQL Server + Redis)

## Quick Start

### 1. Infrastructure (Docker)

```bash
docker compose up -d sqlserver redis
```

### 2. Backend

```bash
cd backend
dotnet restore
dotnet ef database update --project src/NexusERP.Infrastructure --startup-project src/NexusERP.API
dotnet run --project src/NexusERP.API
```

API: `https://localhost:5001` | Swagger: `https://localhost:5001/swagger`

### 3. Frontend

```bash
cd frontend/nexus-erp-web
npm install
npm start
```

App: `http://localhost:4200`

### Default Admin

| Field | Value |
|-------|-------|
| Email | admin@nexuserp.com |
| Password | Admin@123 |

## Modules

Authentication · Dashboard · Users · Roles · Permissions · Projects · Tasks · Kanban · Calendar · Comments · Files · Notifications · Reports · Audit Logs · Settings

## Testing

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend/nexus-erp-web && npm test
```

## License

MIT
