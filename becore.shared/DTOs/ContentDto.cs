namespace becore.shared.DTOs;

public class ContentDto
{
    public Guid Id { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<TagDto> Tags { get; set; } = [];
}

public class ContentFilter
{
    public ushort Count { get; set; }
    public ushort Position { get; set; }
    public ushort ContentType { get; set; }
    public string? Name { get; set; }
    public List<Guid> TagId { get; set; } = [];
}

public class ContentResponse
{
    public ContentFilter? Filter { get; set; }
    public IEnumerable<ContentDto> Content { get; set; } = [];
    public int TotalCount { get; set; }
}