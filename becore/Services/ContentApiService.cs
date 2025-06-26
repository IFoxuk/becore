using System.Text.Json;
using becore.shared.DTOs;

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
    
    public async Task<PageDto?> GetPageByIdAsync(int id)
    {
        try
        {
            // Получаем все страницы и ищем по hash code ID
            var pages = await SearchPagesAsync();
            return pages.FirstOrDefault(p => p.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении страницы по ID: {ex.Message}");
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

    public async Task<SearchResultDto?> SearchContentAsync(string query, int page = 1, int pageSize = 12)
    {
        try
        {
            // Парсим query для извлечения имени и тегов
            var name = "";
            var tag = "";
            
            if (!string.IsNullOrEmpty(query))
            {
                // Ищем теги в формате "tag:tagname"
                var tagMatch = System.Text.RegularExpressions.Regex.Match(query, @"tag:([^\s]+)");
                if (tagMatch.Success)
                {
                    tag = tagMatch.Groups[1].Value;
                    // Удаляем тег из основного запроса
                    query = System.Text.RegularExpressions.Regex.Replace(query, @"tag:[^\s]+", "").Trim();
                }
                name = query.Trim();
            }
            
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
                var pages = JsonSerializer.Deserialize<IEnumerable<PageDto>>(json, _jsonOptions) ?? new List<PageDto>();
                
                // Симулируем пагинацию на клиенте (так как API пока не поддерживает)
                var totalCount = pages.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var pagedItems = pages.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                return new SearchResultDto
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при поиске контента: {ex.Message}");
            return null;
        }
    }
}
