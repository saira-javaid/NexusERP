# NexusERP Features

Complete feature reference for the current application.

## Authentication & authorization

- Email/password login and registration
- JWT access tokens (15 min) with refresh tokens (7 days)
- Permission-based route guards on frontend and `[RequirePermission]` on API
- Role management with assignable permissions
- User activation/deactivation

## Dashboard

- Project and task summary cards
- Recent activity overview
- Configurable auto-refresh interval (Settings)

## Projects

- Paginated project list with search
- Create, edit, view project details
- Status: Planning, Active, On Hold, Completed, Cancelled
- Budget, manager, start/end dates
- Link to Kanban board per project
- Mobile: responsive card-style table rows

## Tasks

- Paginated task list with status filter
- Create, edit, delete tasks (permission-gated)
- Status: Backlog, To Do, In Progress, In Review, Done, Cancelled
- Priority: Low, Medium, High, Critical
- Assignee, due date, estimated/actual hours
- **Custom confirmation modal** before delete (OK / Cancel)
- Mobile: responsive card-style table rows

## Kanban

- Per-project drag-and-drop board by task status
- Optional assignee avatars (Settings toggle)
- Column counts and task cards

## Calendar

- **Desktop:** 7-column month grid with weekday headers, today highlight, status-colored task chips
- **Mobile:** Agenda list — one card per day with due tasks
- Month navigation + **Today** jump button
- Tasks loaded by due date for the visible month

## Meetings

- List with search and status filter
- Schedule new meeting (title, time, location, description, project, attendees)
- Meeting detail: schedule, organizer, location, project, attendee list
- Edit and cancel meeting (with confirmation modal)
- Permissions: `meetings.view`, `.create`, `.edit`, `.delete`

## Users & roles

- User list with search, active/inactive filter, pagination
- Deactivate users with confirmation modal
- Role list with permission counts
- Create/edit roles (system roles protected)
- Delete custom roles with confirmation modal

## Notifications

- Real-time delivery via **SignalR** (`/hubs/notifications`)
- Unread badge on toolbar bell icon
- Notification list: filter All / Unread only
- Mark single or all as read
- Click notification to navigate (action URL)
- **Mobile:** full-width cards, timestamp below message (no truncation)

## Reports

- Summary stat cards: total projects, active projects, total tasks, total budget
- **Projects by status** — breakdown chips + bar chart
- **Tasks by status** — breakdown chips + doughnut chart
- **Project overview table** — code, name, status, task count, budget, manager
- **Pagination** on project overview (12 / 24 / 36 per page)
- Export full project list to Excel

## Audit logs

- Read-only log of create/update/delete actions
- User, entity, timestamp, IP
- Paginated list with responsive mobile layout

## Settings

Stored in browser `localStorage` via `UserPreferencesService`:

| Category | Options |
|----------|---------|
| Appearance | Dark mode, compact table density, collapsed sidebar, Kanban avatars |
| Regional | Date format, default page size (12/24/36) |
| Notifications | Email toggle, desktop notifications, dashboard refresh interval |
| Data | Default export format (Excel/CSV), **confirm before delete/deactivate** |

## AI chat assistant

- Floating action button (bottom-right) on all authenticated pages
- Markdown rendering in assistant replies (bold, lists)
- **Built-in agent** (no API key): queries projects, tasks, meetings, dashboard; creates tasks
- **OpenAI mode** (optional): set `Ai:OpenAi:ApiKey` for GPT-powered replies with tool use
- Project picker buttons when creating a task without a specified project
- Conversation history (last 10 messages) sent with each request

See [CHATBOT.md](CHATBOT.md) for configuration and example prompts.

## UI / UX

| Feature | Description |
|---------|-------------|
| Dark / light theme | Toolbar toggle; persisted in settings |
| Responsive sidebar | Overlay drawer on handset breakpoints |
| Loading bar | Global HTTP loading indicator |
| Confirm dialogs | Material modals for delete/cancel/deactivate |
| Favicon | SVG app icon |
| List pagination | Shared component across Projects, Tasks, Meetings, Users, Roles, Audit Logs, Reports |

## Seed data (development)

First API run seeds:

- 1 admin + multiple sample users across roles
- 15 core projects + **30 extra projects** (45 total)
- Hundreds of tasks across projects with realistic statuses and assignees
- Sample meetings, notifications, and audit log entries

Default login: `admin@nexuserp.com` / `Admin@123`
