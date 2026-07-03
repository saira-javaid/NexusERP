namespace NexusERP.Application.DTOs.Chat;

public record ChatHistoryItem(string Role, string Content);

public record SendChatMessageRequest(
    string Message,
    IReadOnlyList<ChatHistoryItem>? History = null,
    Guid? SelectedProjectId = null,
    string? PendingTaskTitle = null);

public record ProjectChoiceDto(Guid Id, string Name);

public record ChatMessageResponse(
    string Reply,
    IReadOnlyList<string> ToolsUsed,
    string Provider,
    IReadOnlyList<ProjectChoiceDto>? ProjectChoices = null,
    string? PendingTaskTitle = null);
