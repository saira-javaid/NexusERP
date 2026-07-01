using NexusERP.Domain.Common;
using NexusERP.Domain.Enums;

namespace NexusERP.Domain.Entities;

public class ProjectFile : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public FileCategory Category { get; set; } = FileCategory.Document;
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid UploadedById { get; set; }

    public Project? Project { get; set; }
    public TaskItem? Task { get; set; }
    public ApplicationUser UploadedBy { get; set; } = null!;
}
