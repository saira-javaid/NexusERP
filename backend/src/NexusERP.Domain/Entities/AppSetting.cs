using NexusERP.Domain.Common;

namespace NexusERP.Domain.Entities;

public class AppSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
}
