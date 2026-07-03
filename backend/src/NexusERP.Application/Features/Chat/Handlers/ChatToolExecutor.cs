using System.Text.Json;
using MediatR;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Chat;
using NexusERP.Application.DTOs.Meetings;
using NexusERP.Application.DTOs.Projects;
using NexusERP.Application.DTOs.Tasks;
using NexusERP.Application.Features.Meetings.Queries;
using NexusERP.Application.Features.Projects.Queries;
using NexusERP.Application.Features.Tasks.Commands;
using NexusERP.Application.Features.Tasks.Queries;
using NexusERP.Domain.Enums;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Application.Features.Chat.Handlers;

public class ChatToolExecutor
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ChatToolExecutor(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ProjectChoiceDto>> GetProjectChoicesAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission("projects.view"))
            return Array.Empty<ProjectChoiceDto>();

        var result = await _mediator.Send(new GetProjectsQuery(1, 20), cancellationToken);
        return result.Items.Select(p => new ProjectChoiceDto(p.Id, p.Name)).ToList();
    }

    public async Task<string> ExecuteAsync(string toolName, JsonElement? arguments, CancellationToken cancellationToken)
    {
        return toolName switch
        {
            "query_projects" => await QueryProjectsAsync(arguments, cancellationToken),
            "query_tasks" => await QueryTasksAsync(arguments, cancellationToken),
            "create_task" => await CreateTaskAsync(arguments, cancellationToken),
            "query_meetings" => await QueryMeetingsAsync(arguments, cancellationToken),
            "get_dashboard_stats" => await GetDashboardStatsAsync(cancellationToken),
            _ => $"Unknown tool: {toolName}"
        };
    }

    private async Task<string> QueryProjectsAsync(JsonElement? args, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission("projects.view"))
            return "You don't have permission to view projects.";

        var search = args?.TryGetProperty("search", out var s) == true ? s.GetString()?.Trim() : null;
        var pageSize = string.IsNullOrWhiteSpace(search) ? 8 : 20;
        var result = await _mediator.Send(new GetProjectsQuery(1, pageSize, search), cancellationToken);

        if (result.Items.Count == 0)
            return string.IsNullOrWhiteSpace(search)
                ? "No projects found."
                : $"No project found matching **{search}**.";

        if (!string.IsNullOrWhiteSpace(search))
        {
            var matches = result.Items.Where(p =>
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Code.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matches.Count == 0)
                matches = result.Items.ToList();

            if (matches.Count == 1)
                return FormatProjectDetail(matches[0]);

            if (matches.Count <= 5)
            {
                var lines = matches.Select(p => $"- **{p.Code}** {p.Name} (Status: {p.Status})");
                return $"Found {matches.Count} projects matching **{search}**:\n" + string.Join("\n", lines) +
                       "\n\nBe more specific to see full details for one project.";
            }

            return $"Found {matches.Count}+ projects matching **{search}**. Please narrow your search.";
        }

        var listLines = result.Items.Select(p =>
            $"- **{p.Code}** {p.Name} (Status: {p.Status}, Tasks: {p.TaskCount})");
        return $"Found {result.TotalCount} project(s). Top results:\n" + string.Join("\n", listLines);
    }

    private static string FormatProjectDetail(ProjectDto p)
    {
        var lines = new List<string>
        {
            $"**{p.Name}** ({p.Code})",
            $"- **Status:** {p.Status}",
            $"- **Tasks:** {p.TaskCount}",
            $"- **Team members:** {p.MemberCount}",
        };

        if (!string.IsNullOrWhiteSpace(p.ManagerName))
            lines.Add($"- **Manager:** {p.ManagerName}");
        if (p.StartDate.HasValue)
            lines.Add($"- **Start date:** {p.StartDate:MMM d, yyyy}");
        if (p.EndDate.HasValue)
            lines.Add($"- **End date:** {p.EndDate:MMM d, yyyy}");
        if (p.Budget > 0)
            lines.Add($"- **Budget:** {p.Budget:N0}");
        if (!string.IsNullOrWhiteSpace(p.Description))
            lines.Add($"- **Description:** {p.Description}");

        return string.Join("\n", lines);
    }

    private async Task<string> QueryTasksAsync(JsonElement? args, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission("tasks.view"))
            return "You don't have permission to view tasks.";

        TaskStatus? status = null;
        if (args?.TryGetProperty("status", out var st) == true && st.ValueKind == JsonValueKind.String)
        {
            status = st.GetString()?.ToLowerInvariant() switch
            {
                "todo" => TaskStatus.Todo,
                "inprogress" or "in progress" => TaskStatus.InProgress,
                "done" => TaskStatus.Done,
                _ => null
            };
        }

        var result = await _mediator.Send(new GetTasksQuery(null, 1, 10, status), cancellationToken);
        if (result.Items.Count == 0) return "No tasks found matching your criteria.";

        var lines = result.Items.Select(t =>
            $"- **{t.Title}** [{t.ProjectName}] — {t.Status}, assignee: {t.AssigneeName ?? "unassigned"}");
        return $"Found {result.TotalCount} task(s):\n" + string.Join("\n", lines);
    }

    private async Task<string> CreateTaskAsync(JsonElement? args, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission("tasks.create"))
            return "You don't have permission to create tasks.";

        if (args == null || !args.Value.TryGetProperty("title", out var titleEl) || string.IsNullOrWhiteSpace(titleEl.GetString()))
            return "Please provide a task title to create.";

        var title = titleEl.GetString()!;
        var description = args.Value.TryGetProperty("description", out var d) ? d.GetString() : null;
        Guid? projectId = null;

        if (args.Value.TryGetProperty("projectId", out var pid) && Guid.TryParse(pid.GetString(), out var parsed))
            projectId = parsed;

        if (projectId == null && args.Value.TryGetProperty("projectName", out var pn))
        {
            var search = pn.GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var projects = await _mediator.Send(new GetProjectsQuery(1, 20, search), cancellationToken);
                var match = projects.Items.FirstOrDefault(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    projectId = match.Id;
            }
        }

        if (projectId == null)
            return "Please choose which project this task belongs to.";

        var task = await _mediator.Send(new CreateTaskCommand(new CreateTaskRequest(
            title, description, TaskStatus.Todo, TaskPriority.Medium,
            null, null, null, projectId.Value, null, null, null)), cancellationToken);

        return $"Task created: **{task.Title}** in project **{task.ProjectName}**.";
    }

    private async Task<string> QueryMeetingsAsync(JsonElement? args, CancellationToken cancellationToken)
    {
        if (!_currentUser.HasPermission("meetings.view"))
            return "You don't have permission to view meetings.";

        var search = args?.TryGetProperty("search", out var s) == true ? s.GetString()?.Trim() : null;
        var from = string.IsNullOrWhiteSpace(search) ? DateTime.UtcNow.Date : (DateTime?)null;
        var pageSize = string.IsNullOrWhiteSpace(search) ? 8 : 30;
        var result = await _mediator.Send(new GetMeetingsQuery(1, pageSize, search, From: from), cancellationToken);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var matches = FindMeetingMatches(result.Items, search);

            if (matches.Count == 0)
            {
                var broader = await _mediator.Send(new GetMeetingsQuery(1, 50), cancellationToken);
                matches = FindMeetingMatches(broader.Items, search);
            }

            if (matches.Count == 0)
                return $"No meeting found matching **{search}**.";

            if (matches.Count == 1)
            {
                var detail = await _mediator.Send(new GetMeetingByIdQuery(matches[0].Id), cancellationToken);
                if (detail != null)
                {
                    var prefix = !detail.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                        ? $"Closest match for **{search}**:\n"
                        : "";
                    return prefix + FormatMeetingDetail(detail);
                }
            }

            if (matches.Count <= 5)
            {
                var lines = matches.Select(m => $"- **{m.Title}** — {m.StartAt:g} ({m.AttendeeCount} attendees)");
                return $"Found {matches.Count} meetings matching **{search}**:\n" + string.Join("\n", lines) +
                       "\n\nBe more specific to see full details for one meeting.";
            }

            return $"Found {matches.Count}+ meetings matching **{search}**. Please narrow your search.";
        }

        if (result.Items.Count == 0) return "No upcoming meetings scheduled.";

        var listLines = result.Items.Select(m =>
            $"- **{m.Title}** — {m.StartAt:g} ({m.AttendeeCount} attendees, organizer: {m.OrganizerName})");
        return $"Upcoming meetings:\n" + string.Join("\n", listLines);
    }

    private static List<MeetingDto> FindMeetingMatches(IReadOnlyList<MeetingDto> items, string search)
    {
        var direct = items.Where(m =>
            m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            (m.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

        if (direct.Count > 0) return direct;

        var keywords = ExtractKeywords(search);
        if (keywords.Count == 0) return [];

        var minHits = keywords.Count == 1 ? 1 : Math.Min(2, keywords.Count);
        return items
            .Select(m => new { Meeting = m, Hits = keywords.Count(k => m.Title.Contains(k, StringComparison.OrdinalIgnoreCase)) })
            .Where(x => x.Hits >= minHits)
            .OrderByDescending(x => x.Hits)
            .ThenBy(x => x.Meeting.StartAt)
            .Select(x => x.Meeting)
            .ToList();
    }

    private static List<string> ExtractKeywords(string search) =>
        search.Split([' ', '—', '-', ','], StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim())
            .Where(w => w.Length > 1)
            .ToList();

    private static string FormatMeetingDetail(MeetingDetailDto m)
    {
        var lines = new List<string>
        {
            $"**{m.Title}**",
            $"- **When:** {m.StartAt:g} – {m.EndAt:g}",
            $"- **Status:** {m.Status}",
            $"- **Organizer:** {m.OrganizerName}",
        };

        if (!string.IsNullOrWhiteSpace(m.Location))
            lines.Add($"- **Location:** {m.Location}");
        if (!string.IsNullOrWhiteSpace(m.ProjectName))
            lines.Add($"- **Project:** {m.ProjectName}");
        if (!string.IsNullOrWhiteSpace(m.Description))
            lines.Add($"- **Description:** {m.Description}");
        if (m.Attendees.Count > 0)
        {
            var names = string.Join(", ", m.Attendees.Take(6).Select(a => a.FullName));
            lines.Add($"- **Attendees ({m.Attendees.Count}):** {names}");
        }

        return string.Join("\n", lines);
    }

    private async Task<string> GetDashboardStatsAsync(CancellationToken cancellationToken)
    {
        var projects = await _mediator.Send(new GetProjectsQuery(1, 1), cancellationToken);
        var tasks = await _mediator.Send(new GetTasksQuery(null, 1, 1), cancellationToken);
        var inProgress = await _mediator.Send(new GetTasksQuery(null, 1, 1, TaskStatus.InProgress), cancellationToken);
        var done = await _mediator.Send(new GetTasksQuery(null, 1, 1, TaskStatus.Done), cancellationToken);

        return $"**Dashboard summary**\n" +
               $"- Projects: {projects.TotalCount}\n" +
               $"- Total tasks: {tasks.TotalCount}\n" +
               $"- In progress: {inProgress.TotalCount}\n" +
               $"- Done: {done.TotalCount}";
    }
}
