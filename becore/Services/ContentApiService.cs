using System.Text.Json;
using System.Text;

namespace becore.Services;

public class ContentApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ContentApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<IEnumerable<PageDto>> SearchPagesAsync(string? name = null, string? tag = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(name))
                queryParams.Add($"name={Uri.EscapeDataString(name)}");
            
            if (!string.IsNullOrEmpty(tag))
                queryParams.Add($"tag={Uri.EscapeDataString(tag)}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"api/content/pages{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var pages = JsonSerializer.Deserialize<IEnumerable<PageDto>>(json, _jsonOptions);
                return pages ?? Enumerable.Empty<PageDto>();
            }
            
            return Enumerable.Empty<PageDto>();
        }
        catch (Exception ex)
        {
            // В реальном приложении здесь должно быть логирование
            Console.WriteLine($"Ошибка при поиске страниц: {ex.Message}");
            return Enumerable.Empty<PageDto>();
        }
    }

    public async Task<PageDto?> GetPageByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/content/pages/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PageDto>(json, _jsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении страницы: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetAllTagsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/content/tags");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tags = JsonSerializer.Deserialize<IEnumerable<string>>(json, _jsonOptions);
                return tags ?? Enumerable.Empty<string>();
            }
            
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении тегов: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }
}

// DTO модели для клиентской стороны
public class PageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? QuadIcon { get; set; }
    public string? WideIcon { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime Created { get; set; }
}

public class PageFilterDto
{
    public string? Name { get; set; }
    public string? Tag { get; set; }
}
