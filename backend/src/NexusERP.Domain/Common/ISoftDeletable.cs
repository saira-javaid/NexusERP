namespace NexusERP.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
