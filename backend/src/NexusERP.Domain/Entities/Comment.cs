using NexusERP.Domain.Common;

namespace NexusERP.Domain.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ParentCommentId { get; set; }

    public ApplicationUser Author { get; set; } = null!;
    public TaskItem? Task { get; set; }
    public Project? Project { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
}
