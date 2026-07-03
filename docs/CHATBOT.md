# NexusERP AI Chat Assistant

The floating chat widget provides an in-app assistant for querying ERP data and performing actions.

## How it works

1. User opens the chat panel (robot FAB, bottom-right).
2. Frontend sends `POST /api/chat/message` with the message and recent history.
3. Backend routes to `AgenticChatService`:
   - If `Ai:OpenAi:ApiKey` is set → OpenAI chat completions with tool definitions
   - Otherwise → **built-in rule-based agent** (always available)
4. Tools execute via `ChatToolExecutor` using the logged-in user's permissions.
5. Response includes reply text and optional UI hints (`projectChoices`, `pendingTaskTitle`).

## Configuration

### Built-in agent (default)

No configuration required. Works offline from OpenAI.

### OpenAI (optional)

**appsettings.json** (local):

```json
"Ai": {
  "OpenAi": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini"
  }
}
```

**docker-compose.yml** (API service):

```yaml
Ai__OpenAi__ApiKey: "sk-..."
Ai__OpenAi__Model: "gpt-4o-mini"
```

If OpenAI fails or the key is empty, the service automatically falls back to the built-in agent.

## Available tools

| Tool | Permission required | Description |
|------|-------------------|-------------|
| `query_projects` | `projects.view` | Search/list projects |
| `query_tasks` | `tasks.view` | Search/list tasks by project or status |
| `query_meetings` | `meetings.view` | Search/list upcoming meetings |
| `create_task` | `tasks.create` | Create a task on a project |
| `get_dashboard_stats` | Authenticated | Project/task summary counts |

## Example prompts

```
What projects are active?
Show tasks for Revenue Recognition Module
List my meetings this week
Dashboard summary
Create task Review API documentation
Add task Performance testing for PRJ-SOC-043
Help
```

### Creating tasks

- **With project in message:**  
  `Create task "Update firewall rules" for Security Operations Center`

- **Without project:**  
  `Create task Review docs`  
  → Assistant returns project choice buttons; click one to confirm creation.

## Frontend integration

| File | Role |
|------|------|
| `core/components/chat-widget/` | UI panel, message list, project picker |
| `core/services/chat.service.ts` | HTTP client for `/api/chat/message` |
| `core/pipes/chat-markdown.pipe.ts` | Bold/list formatting in replies |
| `main-layout.component.html` | Embeds `<app-chat-widget />` |

## API contract

**Request** (`SendChatMessageRequest`):

```json
{
  "message": "Show active projects",
  "history": [
    { "role": "user", "content": "Hi" },
    { "role": "assistant", "content": "Hello!" }
  ],
  "selectedProjectId": "optional-guid-when-picking-project",
  "pendingTaskTitle": "optional-title-from-prior-turn"
}
```

**Response** (`ChatMessageResponse`):

```json
{
  "reply": "Here are the active projects...",
  "provider": "NexusERP Agent",
  "toolsUsed": ["query_projects"],
  "projectChoices": [{ "id": "...", "name": "Project A" }],
  "pendingTaskTitle": "Review docs"
}
```

## Security

- All chat endpoints require authentication (`[Authorize]`).
- Tools check permissions before querying or mutating data.
- Task creation uses the same CQRS commands as the REST API.
- Do not expose OpenAI API keys in client-side code — keys stay server-side only.
