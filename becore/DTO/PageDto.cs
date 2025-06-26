namespace becore.DTO;

/// <summary>
/// DTO для отображения страниц контента в интерфейсе
/// </summary>
public class PageDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int ViewCount { get; set; }
    public int DownloadCount { get; set; }
}

/// <summary>
/// DTO для ответа с пагинацией
/// </summary>
public class SearchResultDto
{
    public List<PageDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
