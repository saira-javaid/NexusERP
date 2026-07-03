# NexusERP Architecture

## Overview

NexusERP follows **Clean Architecture** with strict dependency rules: inner layers never depend on outer layers.

```
┌─────────────────────────────────────────────────────────┐
│                    NexusERP.API                          │
│  Controllers · Middleware · Swagger · Health Checks      │
├─────────────────────────────────────────────────────────┤
│               NexusERP.Infrastructure                    │
│  EF Core · Identity · JWT · Redis · SignalR · AI Chat   │
├─────────────────────────────────────────────────────────┤
│               NexusERP.Application                       │
│  CQRS (MediatR) · DTOs · Validators · ChatToolExecutor   │
├─────────────────────────────────────────────────────────┤
│                  NexusERP.Domain                         │
│  Entities · Enums · Repository Interfaces                │
└─────────────────────────────────────────────────────────┘
```

## Design patterns

| Pattern | Implementation |
|---------|----------------|
| CQRS | MediatR commands/queries per feature |
| Repository | Generic `IRepository<T>` + specific repos |
| Unit of Work | `IUnitOfWork` coordinates repositories |
| Pipeline behaviors | Validation, logging via MediatR |
| JWT + refresh | Access (15 min) + refresh (7 days) tokens |
| Soft delete | Global query filters on entities |
| Audit trail | `IAuditService` logs all mutations |
| Agentic chat | `IAiChatService` → OpenAI or built-in rule agent + `ChatToolExecutor` |

## API endpoints

| Module | Base route | Notes |
|--------|-----------|-------|
| Auth | `/api/auth` | Login, refresh, register |
| Projects | `/api/projects` | CRUD, pagination, search |
| Tasks | `/api/tasks` | CRUD, calendar range, filters |
| Meetings | `/api/meetings` | CRUD, date/status filters |
| Dashboard | `/api/dashboard` | Summary stats |
| Notifications | `/api/notifications` | List, mark read, pagination |
| Reports | `/api/reports` | `GET /overview` — summary + project rows |
| Chat | `/api/chat` | `POST /message` — AI assistant |
| Users | `/api/users` | User management |
| Roles | `/api/roles` | Role + permission assignment |
| Files | `/api/files` | Upload/download attachments |
| Audit Logs | `/api/auditlogs` | Read-only audit trail |
| SignalR | `/hubs/notifications` | Real-time notification push |
| Health | `/health` | Health check |

All routes except auth require a valid JWT. Most write operations require specific permissions via `[RequirePermission]`.

## Domain entities

| Entity | Purpose |
|--------|---------|
| Project | Code, name, status, budget, manager, dates |
| TaskItem | Title, status, priority, assignee, due date, hours |
| Meeting | Title, schedule, organizer, attendees, optional project |
| ApplicationUser / Role | Identity with fine-grained permissions |
| Notification | In-app alerts with optional action URL |
| AuditLog | Who changed what and when |
| FileAttachment | Linked file metadata |

## Permissions

Permissions are stored in the database and embedded in JWT claims. Examples:

| Permission | Module |
|------------|--------|
| `projects.view` / `.create` / `.edit` / `.delete` | Projects |
| `tasks.view` / `.create` / `.edit` / `.delete` | Tasks |
| `meetings.view` / `.create` / `.edit` / `.delete` | Meetings |
| `users.manage` | Users |
| `roles.manage` | Roles |
| `reports.view` | Reports |
| `audit.view` | Audit logs |
| `settings.manage` | Settings |

Angular `permissionGuard` and `AuthService.hasPermission()` mirror API enforcement.

### Default roles

| Role | Typical access |
|------|----------------|
| Admin | Full access |
| Manager | Projects, tasks, meetings, reports, users (view) |
| ProjectLead | Projects + tasks + meetings (create/edit) |
| Member / Developer | View + create/edit own work |
| Viewer / Client | Read-only |

## Frontend architecture

```
src/app/
├── core/
│   ├── components/chat-widget/    # Floating AI assistant
│   ├── layout/main-layout/        # Sidenav, toolbar, responsive shell
│   ├── services/                  # API, auth, SignalR, chat, theme, preferences
│   ├── guards/                    # authGuard, permissionGuard
│   └── interceptors/              # JWT, loading, error handling
├── shared/
│   ├── components/
│   │   ├── list-pagination/       # Shared mat-paginator wrapper
│   │   └── confirm-dialog/        # Material delete/cancel confirmations
│   ├── pipes/                     # statusLabel, priorityLabel, etc.
│   └── services/                  # export.service, confirm-dialog.service
└── features/                      # Lazy-loaded feature areas
    ├── auth/
    ├── dashboard/
    ├── projects/
    ├── tasks/
    ├── kanban/
    ├── calendar/
    ├── meetings/
    ├── users/
    ├── roles/
    ├── notifications/
    ├── reports/
    ├── audit-logs/
    └── settings/
```

### Responsive UI patterns

| Pattern | Implementation |
|---------|----------------|
| Mobile list tables | `_responsive-table.scss` — card-style rows at ≤768px |
| Calendar | Desktop 7-column grid; mobile agenda list (`BreakpointObserver`) |
| Notifications | Card list with stacked date/time (no truncated meta) |
| Confirmations | `ConfirmDialogService` — replaces `window.confirm()` |
| Pagination | `ListPaginationComponent` + user preference page size |

## Chat architecture

```
ChatWidget (Angular)
    → POST /api/chat/message
        → SendChatMessageCommand (MediatR)
            → AgenticChatService
                ├─ OpenAI API (if ApiKey configured)
                └─ Built-in agent (regex + ChatToolExecutor)
                    ├─ query_projects
                    ├─ query_tasks
                    ├─ query_meetings
                    ├─ create_task
                    └─ get_dashboard_stats
```

Tool execution respects the current user's JWT permissions. Task creation can return `projectChoices` for the UI project picker.

## Data seeding

`DataSeederHostedService` runs on API startup:

1. Roles, permissions, role-permission mappings  
2. Admin + sample users  
3. Core sample projects and tasks  
4. **ExtraSeedData** — 30 additional projects (`PRJ-FIN-015` … `PRJ-CDP-044`) and ~140 tasks  
5. Audit logs, notifications, meetings  

Seeding is idempotent — existing data is not duplicated.

## Infrastructure (Docker)

| Container | Image / build | Port |
|-----------|---------------|------|
| `nexuserp-sqlserver` | MSSQL 2022 | 1433 |
| `nexuserp-redis` | Redis 7 | 6379 |
| `nexuserp-api` | Backend Dockerfile | 5000 → 8080 |
| `nexuserp-web` | Frontend Dockerfile (nginx) | 4200 → 80 |

Redis is used for caching/session support. SignalR notifications hub is hosted in the API process.

## Security notes for production

- Replace JWT key and database passwords with environment variables or a secret manager  
- Do not commit real OpenAI API keys  
- Restrict CORS origins  
- Use HTTPS termination (reverse proxy) in front of API and web containers
