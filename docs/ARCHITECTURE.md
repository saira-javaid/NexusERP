# NexusERP Architecture

## Overview

NexusERP follows **Clean Architecture** with strict dependency rules: inner layers never depend on outer layers.

```
┌─────────────────────────────────────────────────────────┐
│                    NexusERP.API                          │
│  Controllers · Middleware · Swagger · Health Checks      │
├─────────────────────────────────────────────────────────┤
│               NexusERP.Infrastructure                    │
│  EF Core · Identity · JWT · Redis · SignalR · Files    │
├─────────────────────────────────────────────────────────┤
│               NexusERP.Application                       │
│  CQRS (MediatR) · DTOs · Validators · AutoMapper       │
├─────────────────────────────────────────────────────────┤
│                  NexusERP.Domain                         │
│  Entities · Enums · Repository Interfaces              │
└─────────────────────────────────────────────────────────┘
```

## Design Patterns

| Pattern | Implementation |
|---------|----------------|
| CQRS | MediatR commands/queries per feature |
| Repository | Generic `IRepository<T>` + specific repos |
| Unit of Work | `IUnitOfWork` coordinates repositories |
| Pipeline Behaviors | Validation, logging via MediatR |
| JWT + Refresh | Access (15min) + Refresh (7 days) tokens |
| Soft Delete | Global query filters on entities |
| Audit Trail | `IAuditService` logs all mutations |

## API Endpoints

| Module | Base Route |
|--------|-----------|
| Auth | `/api/auth` |
| Projects | `/api/projects` |
| Tasks | `/api/tasks` |
| Dashboard | `/api/dashboard` |
| Notifications | `/api/notifications` |
| Files | `/api/files` |
| Audit Logs | `/api/auditlogs` |
| SignalR | `/hubs/notifications` |
| Health | `/health` |

## Frontend Architecture

```
src/app/
├── core/           # Singleton services, guards, interceptors, layout
├── shared/         # Pipes, directives, export utilities
└── features/       # Lazy-loaded feature areas
    ├── auth/
    ├── dashboard/
    ├── projects/
    ├── tasks/
    ├── kanban/
    ├── calendar/
    └── ...
```

## Permissions

Fine-grained permissions are stored in the database and embedded in JWT claims. The `RequirePermission` attribute and Angular `permissionGuard` enforce access at API and UI levels respectively.
