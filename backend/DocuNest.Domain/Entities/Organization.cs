namespace DocuNest.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";
    public string Color { get; set; } = "#6366f1";
    public long Version { get; set; } = 1;
}

public sealed class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";
    public string Color { get; set; } = "#6366f1";
    public long Version { get; set; } = 1;
}

public sealed class DocumentTag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid TagId { get; set; }
    public long Version { get; set; } = 1;
}
