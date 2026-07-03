using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Chat;
using NexusERP.Application.Features.Chat.Handlers;

namespace NexusERP.Infrastructure.Services;

public class AgenticChatService : IAiChatService
{
    private readonly ChatToolExecutor _tools;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgenticChatService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public AgenticChatService(
        ChatToolExecutor tools,
        ICurrentUserService currentUser,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AgenticChatService> logger)
    {
        _tools = tools;
        _currentUser = currentUser;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatMessageResponse> SendMessageAsync(SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Ai:OpenAi:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                return await SendWithOpenAiAsync(request, apiKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI chat failed, falling back to built-in agent");
            }
        }

        return await SendWithBuiltInAgentAsync(request, cancellationToken);
    }

    private async Task<ChatMessageResponse> SendWithBuiltInAgentAsync(
        SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var message = request.Message.Trim();
        var lower = message.ToLowerInvariant();
        var toolsUsed = new List<string>();

        if (request.SelectedProjectId.HasValue && !string.IsNullOrWhiteSpace(request.PendingTaskTitle))
        {
            toolsUsed.Add("create_task");
            var args = JsonSerializer.SerializeToElement(new
            {
                title = request.PendingTaskTitle,
                projectId = request.SelectedProjectId.Value.ToString()
            }, JsonOptions);
            var result = await _tools.ExecuteAsync("create_task", args, cancellationToken);
            return Response(result, toolsUsed, "NexusERP Agent");
        }

        if (Regex.IsMatch(lower, @"\b(help|what can you|capabilities)\b"))
            return Response(HelpText(), toolsUsed, "NexusERP Agent");

        if (Regex.IsMatch(lower, @"\b(dashboard|summary|overview|stats)\b"))
        {
            toolsUsed.Add("get_dashboard_stats");
            var result = await _tools.ExecuteAsync("get_dashboard_stats", null, cancellationToken);
            return Response(result, toolsUsed, "NexusERP Agent");
        }

        if (Regex.IsMatch(lower, @"\b(meeting|meetings|schedule)\b"))
        {
            toolsUsed.Add("query_meetings");
            var search = ExtractMeetingSearchFromMessage(message);
            JsonElement? args = !string.IsNullOrWhiteSpace(search)
                ? JsonSerializer.SerializeToElement(new { search }, JsonOptions)
                : null;
            var result = await _tools.ExecuteAsync("query_meetings", args, cancellationToken);
            return Response(result, toolsUsed, "NexusERP Agent");
        }

        if (Regex.IsMatch(lower, @"\b(create|add|new)\b.*\b(task)\b") ||
            Regex.IsMatch(lower, @"\btask\b.*\b(create|add)\b"))
        {
            toolsUsed.Add("create_task");
            var title = ExtractTaskTitle(message) ?? "New task from assistant";
            var projectName = ExtractProjectNameFromMessage(message);

            if (!string.IsNullOrWhiteSpace(projectName))
            {
                var args = JsonSerializer.SerializeToElement(new { title, projectName }, JsonOptions);
                var result = await _tools.ExecuteAsync("create_task", args, cancellationToken);
                return Response(result, toolsUsed, "NexusERP Agent");
            }

            var choices = await _tools.GetProjectChoicesAsync(cancellationToken);
            if (choices.Count == 0)
                return Response("No projects exist. Create a project first.", toolsUsed, "NexusERP Agent");

            if (choices.Count == 1)
            {
                var args = JsonSerializer.SerializeToElement(new
                {
                    title,
                    projectId = choices[0].Id.ToString()
                }, JsonOptions);
                var result = await _tools.ExecuteAsync("create_task", args, cancellationToken);
                return Response(result, toolsUsed, "NexusERP Agent");
            }

            return Response(
                $"Which project should **{title}** go in? Pick one below:",
                toolsUsed,
                "NexusERP Agent",
                choices,
                title);
        }

        if (Regex.IsMatch(lower, @"\b(task|tasks|todo|assignment)\b"))
        {
            toolsUsed.Add("query_tasks");
            JsonElement? args = null;
            if (lower.Contains("progress")) args = JsonSerializer.SerializeToElement(new { status = "inprogress" }, JsonOptions);
            else if (lower.Contains("done")) args = JsonSerializer.SerializeToElement(new { status = "done" }, JsonOptions);
            var result = await _tools.ExecuteAsync("query_tasks", args, cancellationToken);
            return Response(result, toolsUsed, "NexusERP Agent");
        }

        if (Regex.IsMatch(lower, @"\b(project|projects)\b"))
        {
            toolsUsed.Add("query_projects");
            var search = ExtractProjectSearchFromMessage(message);
            JsonElement? args = !string.IsNullOrWhiteSpace(search)
                ? JsonSerializer.SerializeToElement(new { search }, JsonOptions)
                : null;
            var result = await _tools.ExecuteAsync("query_projects", args, cancellationToken);
            return Response(result, toolsUsed, "NexusERP Agent");
        }

        return Response(
            "I'm your NexusERP assistant. I can look up **projects**, **tasks**, **meetings**, show a **dashboard summary**, or **create tasks**.\n\n" +
            "Try:\n- \"List my projects\"\n- \"Tell me about GoGreen project status\"\n- \"Show tasks in progress\"\n- \"Create task Review API docs\"\n- \"Upcoming meetings\"\n- \"Dashboard summary\"",
            toolsUsed, "NexusERP Agent");
    }

    private async Task<ChatMessageResponse> SendWithOpenAiAsync(
        SendChatMessageRequest request, string apiKey, CancellationToken cancellationToken)
    {
        var model = _configuration["Ai:OpenAi:Model"] ?? "gpt-4o-mini";
        var client = _httpClientFactory.CreateClient("OpenAi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var toolsUsed = new List<string>();
        var messages = BuildOpenAiMessages(request);
        var tools = GetOpenAiToolDefinitions();

        for (var i = 0; i < 4; i++)
        {
            var body = new Dictionary<string, object>
            {
                ["model"] = model,
                ["messages"] = messages,
                ["tools"] = tools,
                ["tool_choice"] = "auto"
            };

            using var content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"OpenAI error: {json}");

            using var doc = JsonDocument.Parse(json);
            var choice = doc.RootElement.GetProperty("choices")[0].GetProperty("message");

            if (choice.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.GetArrayLength() > 0)
            {
                messages.Add(choice);

                foreach (var call in toolCalls.EnumerateArray())
                {
                    var fn = call.GetProperty("function");
                    var name = fn.GetProperty("name").GetString()!;
                    var argsJson = fn.GetProperty("arguments").GetString() ?? "{}";
                    toolsUsed.Add(name);

                    JsonElement? args = null;
                    if (!string.IsNullOrWhiteSpace(argsJson))
                        args = JsonSerializer.Deserialize<JsonElement>(argsJson);

                    var toolResult = await _tools.ExecuteAsync(name, args, cancellationToken);
                    messages.Add(new Dictionary<string, object>
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = call.GetProperty("id").GetString()!,
                        ["content"] = toolResult
                    });
                }
                continue;
            }

            var reply = choice.GetProperty("content").GetString() ?? "I couldn't generate a response.";
            return Response(reply, toolsUsed, "OpenAI");
        }

        return Response("I reached the maximum number of tool steps. Please try a simpler question.", toolsUsed, "OpenAI");
    }

    private static List<object> BuildOpenAiMessages(SendChatMessageRequest request)
    {
        var messages = new List<object>
        {
            new Dictionary<string, string>
            {
                ["role"] = "system",
                ["content"] = """
                    You are NexusERP Assistant, an agentic AI helper inside an enterprise ERP app.
                    Use tools to fetch real data before answering. Respect user permissions implied by tool errors.
                    Be concise, use markdown lists when listing items. For create_task, ensure a clear title is provided.
                    Current user is authenticated; do not fabricate project or task data.
                    """
            }
        };

        if (request.History != null)
        {
            foreach (var item in request.History.TakeLast(10))
            {
                if (item.Role is "user" or "assistant")
                    messages.Add(new Dictionary<string, string> { ["role"] = item.Role, ["content"] = item.Content });
            }
        }

        messages.Add(new Dictionary<string, string> { ["role"] = "user", ["content"] = request.Message });
        return messages;
    }

    private static object[] GetOpenAiToolDefinitions() =>
    [
        Tool("query_projects", "List projects", new { type = "object", properties = new { search = new { type = "string" } } }),
        Tool("query_tasks", "List tasks", new { type = "object", properties = new { status = new { type = "string", description = "todo, inprogress, or done" } } }),
        Tool("create_task", "Create a new task", new
        {
            type = "object",
            properties = new
            {
                title = new { type = "string" },
                description = new { type = "string" },
                projectId = new { type = "string", description = "Project ID (internal)" },
                projectName = new { type = "string", description = "Project name or code" }
            },
            required = new[] { "title" }
        }),
        Tool("query_meetings", "List or search meetings", new
        {
            type = "object",
            properties = new { search = new { type = "string", description = "Meeting title or keywords" } }
        }),
        Tool("get_dashboard_stats", "Get dashboard counts", new { type = "object", properties = new { } })
    ];

    private static Dictionary<string, object> Tool(string name, string description, object parameters) => new()
    {
        ["type"] = "function",
        ["function"] = new Dictionary<string, object>
        {
            ["name"] = name,
            ["description"] = description,
            ["parameters"] = parameters
        }
    };

    private static ChatMessageResponse Response(
        string reply,
        List<string> toolsUsed,
        string provider,
        IReadOnlyList<ProjectChoiceDto>? projectChoices = null,
        string? pendingTaskTitle = null) =>
        new(reply, toolsUsed, provider, projectChoices, pendingTaskTitle);

    private static string HelpText() =>
        """
        **NexusERP Assistant** can act on your live data:
        - `query_projects` — search/list projects
        - `query_tasks` — filter tasks by status
        - `create_task` — add a task (requires permission)
        - `query_meetings` — upcoming meetings
        - `get_dashboard_stats` — counts overview

        Ask in natural language, e.g. "Show active tasks" or "Create task Deploy staging build".
        """;

    private static string? ExtractQuotedText(string message)
    {
        var match = Regex.Match(message, @"[""']([^""']+)[""']");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string? ExtractTaskTitle(string message)
    {
        var raw = ExtractQuotedText(message) ?? ExtractAfterKeyword(message, "task");
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var inMatch = Regex.Match(raw, @"^(.+?)\s+(?:in|for|on)\s+(?:project\s+)?.+$", RegexOptions.IgnoreCase);
        return (inMatch.Success ? inMatch.Groups[1].Value : raw).Trim();
    }

    private static string? ExtractProjectNameFromMessage(string message)
    {
        var match = Regex.Match(message, @"\b(?:in|for|on)\s+(?:project\s+)?(.+)$", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractProjectSearchFromMessage(string message)
    {
        var quoted = ExtractQuotedText(message);
        if (quoted != null) return quoted;

        var lower = message.ToLowerInvariant();
        if (Regex.IsMatch(lower, @"\b(list|show|all)\s+(all\s+)?projects?\b") && !lower.Contains("about"))
            return null;

        var about = Regex.Match(message, @"\babout\s+(.+?)(?:\s+projects?)?(?:\s+status)?\s*$", RegexOptions.IgnoreCase);
        if (about.Success)
        {
            var term = CleanProjectSearchTerm(about.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(term)) return term;
        }

        var nameBefore = Regex.Match(message,
            @"(?:please\s+)?(?:tell me|what is|how is|status of)\s+(?:about\s+)?(.+?)\s+projects?\b",
            RegexOptions.IgnoreCase);
        if (nameBefore.Success)
        {
            var term = CleanProjectSearchTerm(nameBefore.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(term) && !IsGenericListPhrase(term)) return term;
        }

        var nameBeforeShort = Regex.Match(message, @"\b([\w][\w\s-]{1,40}?)\s+projects?\s+status\b", RegexOptions.IgnoreCase);
        if (nameBeforeShort.Success)
        {
            var term = CleanProjectSearchTerm(nameBeforeShort.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(term)) return term;
        }

        var nameAfter = Regex.Match(message, @"\bprojects?\s+(.+?)(?:\s+status)?\s*$", RegexOptions.IgnoreCase);
        if (nameAfter.Success)
        {
            var term = CleanProjectSearchTerm(nameAfter.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(term) && !IsGenericListPhrase(term)) return term;
        }

        return null;
    }

    private static string CleanProjectSearchTerm(string raw) =>
        Regex.Replace(raw, @"\b(please|tell|me|the|a|an|show|what|is|how|status|about|of|info|information|on|details|for)\b",
            " ", RegexOptions.IgnoreCase).Trim();

    private static bool IsGenericListPhrase(string term) =>
        Regex.IsMatch(term, @"^(list|all|my|show|every|upcoming)$", RegexOptions.IgnoreCase);

    private static string? ExtractMeetingSearchFromMessage(string message)
    {
        var quoted = ExtractQuotedText(message);
        if (quoted != null) return quoted;

        var lower = message.ToLowerInvariant();
        if (Regex.IsMatch(lower, @"\b(list|show|all|upcoming)\s+(all\s+)?meetings?\b") && !lower.Contains("about"))
            return null;

        var about = Regex.Match(message, @"\babout\s+(.+?)(?:\s+meetings?)?\s*$", RegexOptions.IgnoreCase);
        if (about.Success)
        {
            var term = CleanMeetingSearchTerm(about.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(term)) return term;
        }

        var nameBefore = Regex.Match(message,
            @"(?:please\s+)?(?:tell me|what is|details of)\s+(?:about\s+)?(.+?)\s+meetings?\b",
            RegexOptions.IgnoreCase);
        if (nameBefore.Success)
        {
            var term = CleanMeetingSearchTerm(nameBefore.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(term)) return term;
        }

        return null;
    }

    private static string CleanMeetingSearchTerm(string raw) =>
        Regex.Replace(raw, @"\b(please|tell|me|the|a|an|show|what|is|about|of|info|information|on|details|for)\b",
            " ", RegexOptions.IgnoreCase).Trim();

    private static string? ExtractAfterKeyword(string message, string keyword)
    {
        var match = Regex.Match(message, $@"{keyword}\s+(.+)$", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}
